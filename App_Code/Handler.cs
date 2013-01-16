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

        // Update Check, Ping Request, Event Check
        // positive
        // negative
        if ("https://client.vitruvian.biz/update2" == Request.Url.ToString() || "http://client.vitruvian.biz/update2" == Request.Url.ToString())
        {
            Response.ContentType = "text/xml";
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<response protocol=\"3.0\" server=\"prod\">");

            Stream stream = Request.InputStream;
            StreamReader reader = new StreamReader(stream, Request.ContentEncoding);
            string text = reader.ReadToEnd();

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
                                            appId.Append(xmlReader.Value);
                                        }
                                        if (xmlReader.Name == "version")
                                        {
                                        }
                                    }
                                    xmlReader.MoveToElement();
                                }
                                writer.WriteStartElement(xmlReader.Name);
                                break;
                            case XmlNodeType.Text:
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

            // number of seconds since midnight
            int secs = (int)System.DateTime.Now.TimeOfDay.TotalSeconds;

            sb.Append("<daystart elapsed_seconds=\"" + secs.ToString() + "\"/>");
            sb.Append("<app appid=\"" + appId.ToString() + "\" status=\"ok\">");

            if (isUpdateCheck)
            {
                if (false)
                {
                    // no update
                    sb.Append("<updatecheck status=\"noupdate\"/>");
                }
                else
                {
                    // wants sha1 hash
                    //"aa42eeb11c54cf9cacd2385ea1c3c7974826ca1a"

                    sb.Append("<updatecheck status=\"ok\">");
                    sb.Append("<urls>");
                    sb.Append("<url codebase=\"http://dl.vitruvian.biz/\"/>");
                    sb.Append("</urls>");
                    sb.Append("<manifest version=\"1.5.0.0\">");
                    sb.Append("<packages>");
                    sb.Append("<package hash=\"qkLusRxUz5ys0jheocPHl0gmyho=\" name=\"infoatoms-setup.exe\" required=\"true\" size=\"1195336\"/>");
                    sb.Append("</packages>");
                    sb.Append("<actions>");
                    sb.Append("<action arguments=\"\" event=\"install\" run=\"infoatoms-setup.exe\"/>");
                    sb.Append("<action version=\"1.5.0.0\" event=\"postinstall\" onsuccess=\"exitsilentlyonlaunchcmd\"/>");
                    sb.Append("</actions>");
                    sb.Append("</manifest>");
                    sb.Append("</updatecheck>");
                }
            }
            else if (isEventReport)
            {
                sb.Append("<event status=\"ok\"/>");
            }
            sb.Append("</app>");
            sb.Append("</response>");
            Response.Write(sb);
        }
        //for the other calls, check2, etc we don't know what they want, the best we can do is not throw a server error
        else
        {

        }
    }
    public bool IsReusable
    {
        // To enable pooling, return true here.
        // This keeps the handler in memory.
        get { return false; }
    }
}
