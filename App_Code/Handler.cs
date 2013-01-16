using System.Web;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;

public class Handler : IHttpHandler
{
    public Handler()
    {
    }
    /* defined in omaha\base\const_addresses.h:
     * 
     *  Update checks and manifest requests.
     *   https://client.vitruvian.biz/update2
     *   
     *  Pings.
     *   http://client.vitruvian.biz/update
     *   
     * Crash reports.
     *   http://client.vitruvian.biz/report
     *   
     * More information url.
     * Must allow query parameters to be appended to it.
     *   http://www.vitruvian.biz/installer/?
     *   
     * Code Red check url.
     *   http://client.vitruvian.biz/check2
     *   
     * Usage stats url.
     *   http://client.vitruvian.biz/usagestats
     */
    public void ProcessRequest(HttpContext context)
    {
        HttpRequest Request = context.Request;
        HttpResponse Response = context.Response;

        //if (Request.ContentType == "text/xml")
        {

            //System.Net.HttpWebRequest myRequest;

            Response.ContentType = "text/xml";
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<response protocol=\"3.0\" server=\"prod\">");

            // parse request
            //Request.ContentType == 
            //Request.ContentLength == 
            //Request.RequestType == 
            //Request.Url ==
            //Request.Method ==
            //Request.IsSecureConnection
            //Request.QueryString
            //Request.RequestContext

            Stream stream = Request.InputStream;
            StreamReader reader = new StreamReader(stream, Request.ContentEncoding);
            string text = reader.ReadToEnd();

            System.Diagnostics.Debug.WriteLine(text);

            //sb.Append(text);
            StringBuilder output = new StringBuilder();
            StringBuilder version = new StringBuilder();
            StringBuilder appId = new StringBuilder();
            bool isUpdateCheck = false;
            bool isEventReport = false;

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(text)))
            {
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(output, ws))
                {
                    // Parse the file and display each of the nodes.
                    while (xmlReader.Read())
                    {
                        switch (xmlReader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (xmlReader.Name == "updatecheck")
                                {
                                    isUpdateCheck = true;
                                }
                                else if (xmlReader.Name == "event")
                                {
                                    isEventReport = true;
                                }

                                if (xmlReader.HasAttributes)
                                {
                                    for (int i = 0; i < xmlReader.AttributeCount; i++)
                                    {
                                        xmlReader.MoveToAttribute(i);
                                        if (xmlReader.Name == "appid")
                                        {
                                            // is wrapped in { }
                                            //Console.WriteLine(" {0}={1}", xmlReader.Name, xmlReader.Value);
                                            appId.Append(xmlReader.Value);
                                        }
                                        if (xmlReader.Name == "version")
                                        {
                                            //Console.WriteLine(" {0}={1}", xmlReader.Name, xmlReader.Value);
                                            //sb.Append(xmlReader.Value);
                                        }
                                    }
                                    xmlReader.MoveToElement();
                                }
                                //sb.Append(xmlReader.Name);
                                writer.WriteStartElement(xmlReader.Name);
                                break;
                            case XmlNodeType.Text:
                                //sb.Append(xmlReader.Value);
                                writer.WriteString(xmlReader.Value);
                                break;
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                                writer.WriteProcessingInstruction(xmlReader.Name, xmlReader.Value);
                                break;
                            case XmlNodeType.Comment:
                                writer.WriteComment(xmlReader.Value);
                                break;
                            case XmlNodeType.EndElement:
                                writer.WriteFullEndElement();
                                break;
                        }
                    }
                }
            }

            // Update Check, Ping Request, Event Check
            // positive
            // negative
            if ("https://client.vitruvian.biz/update2" == Request.Url.ToString() || "http://client.vitruvian.biz/update2" == Request.Url.ToString())
            {
                // number of seconds since midnight
                int secs = (int)System.DateTime.Now.TimeOfDay.TotalSeconds;

                sb.Append("<daystart elapsed_seconds=\"" + secs.ToString() + "\"/>");
                sb.Append("<app appid=\"" + appId.ToString() + "\" status=\"ok\">");

                //sb.Append(Request.RequestContext.ToString());
                if (isUpdateCheck)
                {
                    if (false)
                    {
                        // no update
                        sb.Append("<updatecheck status=\"noupdate\"/>");
                    }
                    else
                    {
/*
 * <?xml version="1.0" encoding="UTF-8"?>
 * <response protocol="3.0" server="prod">
 * <daystart elapsed_seconds="42774"/>
 * <app appid="{8A69D345-D564-463C-AFF1-A69D9E530F96}" status="ok">
 * <updatecheck status="ok">
 * <urls>
 * <url codebase="http://cache.pack.google.com/edgedl/chrome/win/FF93E464A075D0E7/"/>
 * <url codebase="http://www.google.com/dl/chrome/win/FF93E464A075D0E7/"/>
 * <url codebase="https://dl.google.com/chrome/win/FF93E464A075D0E7/"/>
 * <url codebase="http://dl.google.com/chrome/win/FF93E464A075D0E7/"/>
 * <url codebase="http://google.com/dl/chrome/win/FF93E464A075D0E7/"/>
 * </urls>
 * <manifest version="23.0.1271.97">
 * <packages>
 * <package hash="vvRHaljhwCg1XFC1WVKQKOxYk0E=" name="23.0.1271.97_chrome_installer.exe" required="true" size="30646264"/>
 * </packages>
 * <actions>
 * <action arguments="--multi-install --chrome --verbose-logging --do-not-launch-chrome" event="install" run="23.0.1271.97_chrome_installer.exe"/>
 * <action Version="23.0.1271.97" event="postinstall" onsuccess="exitsilentlyonlaunchcmd"/>
 * </actions>
 * </manifest>
 * </updatecheck>
 * </app>
 * </response>
 */
// wants sha1 hash
//"aa42eeb11c54cf9cacd2385ea1c3c7974826ca1a"

                        sb.Append("<updatecheck status=\"ok\">");
                        sb.Append("<urls>");
                        sb.Append("<url codebase=\"http://dl.vitruvian.biz/\"/>");
                        sb.Append("</urls>");
                        sb.Append("<manifest version=\"1.5.0\">");
                        sb.Append("<packages>");
                        sb.Append("<package hash=\"qkLusRxUz5ys0jheocPHl0gmyho=\" name=\"infoatoms-setup.exe\" required=\"true\" size=\"1195336\"/>");
                        sb.Append("</packages>");
                        sb.Append("<actions>");
                        sb.Append("<action arguments=\"\" event=\"install\" run=\"infoatoms-setup.exe\"/>");
                        sb.Append("<action version=\"1.5.0\" event=\"postinstall\" onsuccess=\"exitsilentlyonlaunchcmd\"/>");
                        sb.Append("</actions>");
                        sb.Append("</manifest>");
                        sb.Append("</updatecheck>");
                    }
                    //sb.Append("<ping status=\"ok\"/>");
                }
                else if (isEventReport)
                {
                    sb.Append("<event status=\"ok\"/>");
                }
                sb.Append("</app>");
            }

            // Event Report
            // can just respond 200 for now

            // Data Request
            //

            sb.Append("</response>");
            Response.Write(sb);
            //Response.Write(text);
        }
    }
    public bool IsReusable
    {
        // To enable pooling, return true here.
        // This keeps the handler in memory.
        get { return false; }
    }
}
