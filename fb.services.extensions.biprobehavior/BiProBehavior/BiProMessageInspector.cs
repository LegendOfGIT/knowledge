using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace FB.Services.Extensions.Behaviors
{
    public class BiProMessageInspector : IClientMessageInspector
    {
        public bool? UseToken { get; set; }
        public string Tokenpath { get; set; }
        public string TokenTemplatepath { get; set; }
        public string TokenURL { get; set; }
        public string TokenUsername { get; set; }
        public string TokenPassword { get; set; }
        public bool? NuernbergerBiPROWorkaround { get; set; }

        public string Namespaces { get; set; }
        public bool? RepairAttributePrefixes { get; set; }

        private XNamespace Tarifierung = "http://www.bipro.net/namespace/tarifierung";
        private XNamespace Transfert = "http://www.bipro.net/namespace/transfer";
        private XNamespace Namespace = "http://www.w3.org/2000/xmlns/";
        private XNamespace Nsi = "http://www.w3.org/2001/XMLSchema-instance";

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            Console.WriteLine("SOAP Response: {0}", reply.ToString());
        }
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var isCustomsecurity = !UseToken.HasValue || UseToken.Value;
            if (isCustomsecurity)
            {
                request.Headers.Clear();

                var token = string.Empty;
                if (isCustomsecurity)
                {
                    //  Wenn das Security-Token noch nicht älter als zwei Stunden ist und bereits als Datei vorliegt, lade es aus der Datei.
                    if (!string.IsNullOrEmpty(Tokenpath) && File.Exists(Tokenpath) && File.GetCreationTime(Tokenpath) > DateTime.Now.AddHours(-2))
                    { 
                        token = File.ReadAllText(Tokenpath);
                    }
                    else
                    { 
                        token = GetAuthenticationToken();
                    }
                }

                request.Headers.Add(
                    MessageHeader.CreateHeader(
                        //  <Security> Knoten
                        "Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
                        //  SecurityToken-Knoten
                        isCustomsecurity ?
                        new SecurityToken {
                            SecurityContextToken = new SecurityContextToken { Identifier = token }
                        } :
                        null
                    )
                );
            }

            var memorystream = new MemoryStream{ };
            var writer = XmlWriter.Create(memorystream);
            request.WriteMessage(writer);
            writer.Flush();
            memorystream.Position = 0;

            var doc = XDocument.Load(memorystream);
            var attribute = doc.Descendants().Attributes(Namespace + "i").FirstOrDefault();
            if (attribute != null)
            {
                attribute.Remove();
            }

            //  Namespaces "aufräumen"
            Cleanup(doc);

            //  ggf. Nürnberger BiPRO Workaround anwenden
            doc = ApplyNuernbergerBiPROWorkaround(doc);

            memorystream.SetLength(0);

            writer = XmlWriter.Create(memorystream);
            doc.WriteTo(writer);
            writer.Flush();
            memorystream.Position = 0;

            var reader = XmlReader.Create(memorystream);
            request = Message.CreateMessage(reader, int.MaxValue, request.Version);
            request.Headers.Action =
                doc.Descendants(Tarifierung + "getQuote").Any() ? "urn:getQuote" :
                doc.Descendants(Tarifierung + "getOffer").Any() ? "urn:getOffer" :
                doc.Descendants(Tarifierung + "getOrder").Any() ? "urn:getOrder" :
                doc.Descendants(Tarifierung + "setOrder").Any() ? "urn:setOrder" :
                doc.Descendants(Transfert + "getShipment").Any() ? "urn:getShipment" :
                request.Headers.Action
            ;

            return default(object);
        }

        private void Cleanup(XDocument doc)
        {
            //  Alle Elemente mit dem vorkommen "xsi:nil='true'" entfernen
            doc.Descendants().Where(desc => desc.Attribute(Nsi + "nil") != null && desc.Attribute(Nsi + "nil").Value == "true").Remove();

            CleanupNamespaces(doc);
        }
        private void CleanupNamespaces(XDocument doc)
        {
            //  Automatische Namensvergabe der Namensprefixe im Rahmen der Proxyklasse "aufräumen"
            if (!string.IsNullOrEmpty(Namespaces))
            {
                var namespaces = new List<Tuple<string, string>>();
                Namespaces.Split(';').ToList().ForEach(
                    @namespace => {
                        var prefix = @namespace.Split('|');
                        if (prefix != null && prefix.Length == 2)
                        { 
                            namespaces.Add(new Tuple<string, string>(prefix[0], prefix[1]));
                        }
                    }
                );

                namespaces.ForEach(
                    @namespace => 
                    {
                        CleanupNamespace(doc, @namespace.Item1, @namespace.Item2);
                    }
                );
            }

            //  Automatisches Ergänzen von Namespaceprefixen in Attributen -MarSch- 06.05.2014
            if (RepairAttributePrefixes.HasValue && RepairAttributePrefixes.Value)
            {
                //  Durchlaufe jedes XML-Attribut
                doc.Descendants().Attributes().ToList().ForEach(
                    attribute => 
                    {
                        //  Attribut enthält keinen Namespaceprefix
                        if (!attribute.Value.Contains(":"))
                        {
                            var parent = attribute.Parent;
                            //  Hole Elternknoten und fahre fort, wenn es sich nicht um den Root-Knoten handelt
                            if (parent != null && parent.Parent != null)
                            {
                                var tokens = parent.ToString().Split(':');
                                var parentnamespace = tokens != null && tokens.Length > 0 ? tokens[0] : string.Empty;
                                parentnamespace = parentnamespace.StartsWith("<") ? parentnamespace.Replace("<", string.Empty) : string.Empty;
                                if (!string.IsNullOrEmpty(parentnamespace))
                                {
                                    attribute.Value = parentnamespace + ":" + attribute.Value;
                                }
                            }
                        }
                    }
                );
            }
        }
        private void CleanupNamespace(XDocument doc, string prefix, string namespaceurl)
        {
            var set = default(XElement);
            var attribute = default(XAttribute);

            var actions = new List<XName> {
                Tarifierung + "getQuote",
                Tarifierung + "getOffer",
                Tarifierung + "getOrder",
                Tarifierung + "setOrder",
                Transfert + "getShipment"
            };

            //  Füge allen Actionelementen, ein Attribut eine eigene Definition des aufzuräumenden Namespaces hinzu.
            foreach (var action in actions)
            {
                set = doc.Descendants(action).FirstOrDefault();
                attribute = set == null ? null : set.Attributes(Namespace + prefix).FirstOrDefault();
                if (set != null && attribute == null)
                {
                    set.Add(new XAttribute(Namespace + prefix, namespaceurl));
                }
            }

            //  Selektiere ...
            doc.
                //  ... alle Elemente des XmlDocument 'doc' ...
                Descendants().
                    //  ... deren Name nicht einer der festgelegten Namen aus der Liste 'actions' entspricht
                    Where(el => !actions.Contains(el.Name)).
                        //  Selektiere aus dieser Menge alle XmlAttribute ...
                        Attributes().
                            //  ... deren Namespace dem aufzuräumenden Namespace 'namespaceurl' entsprechen
                            Where(attr => attr.Value == namespaceurl)
                                //  Durchlaufe alle gefundenen Xml-Attribute mit dem aufzuräumenden Namespace 'namespaceurl'
                                .ToList().ForEach(attr => 
                                {
                                    //  Ermittele aktuelles Prefix für den Namespace
                                    var namespaceprefix = attr.Name.LocalName;

                                    //  XSI-Definitionen, die auf einen Typen mit Namespaceprefix verweisen.
                                    //  Selektiere ...
                                    doc.
                                        //  ... alle Elemente des XmlDocument 'doc' ...
                                        Descendants().
                                            //  ... alle Attribute der selktierten Elemente
                                            Attributes().
                                                ToList().
                                                    //  Durchlaufe gefundene Attribute
                                                    ForEach(a => 
                                                    {
                                                        //  Wenn der Wert des Attribut mit dem aufzuräumenden Namespaceprefix beginnt ...
                                                        if (a.Value.StartsWith(namespaceprefix + ":"))
                                                        {
                                                            //  ... ersetze den gefundenen Namespacepräfix durch den vorgegebenen Präfix.
                                                            a.Value = a.Value.Replace(namespaceprefix + ":", prefix + ":");
                                                        }
                                                    });
                                }
            );

            //  Selektiere ...
            doc.
                //  ... alle Elemente des XmlDocument 'doc' ...
                Descendants().
                    //  ... deren Name nicht einer der festgelegten Namen aus der Liste 'actions' entspricht
                    Where(el => !actions.Contains(el.Name))
                        //  Selektiere alle Attribute der zuvor selektierten Elemente
                        .Attributes().
                            //  .. die eine eigene Definition des aufzuräumenden Namespace herstellen ...
                            Where(attr => attr.Value == namespaceurl)
                                //  ... und entferne diese
                                .Remove()
            ;
        }
        private XDocument ApplyNuernbergerBiPROWorkaround(XDocument doc){
            if (NuernbergerBiPROWorkaround.HasValue && NuernbergerBiPROWorkaround.Value)
            {
                var content = doc.ToString();
                var match = default(Match);

                //  VorgabeBerechnung => ArtID => VorgabeBerechnungErweitert (Namespacestabilisierung)
                match = Regex.Match(content, "<.*?ArtID.*?VorgabeBerechnungErweitert.*?</.*?ArtID>");
                if (match != null && match.Success)
                {
                    var targetnamespace =
                        //  Erweiterungen für Lebenprodukte (Riester)
                        match.Value.Contains("MaximaleRente") || match.Value.Contains("Beitragsfinanzierung") || match.Value.Contains("NettobeitragGesamt") ?
                        "nvg-leben" :
                        //  Alle weiteren erweiterten Vorgabearten
                        "nvg-allgemein"
                    ;

                    //  Umsteuerung des vorherigen auf den Zielnamespace
                    content = 
                        content.
                            Replace(
                                match.Value, 
                                match.Value.
                                    Replace("nvg-kranken", targetnamespace).
                                    Replace("nvg-leben", targetnamespace).
                                    Replace("nvg-allgemein", targetnamespace)
                            )
                    ;
                }

                doc = XDocument.Parse(content);
            }

            return doc;
        }

        /// <summary>
        /// Holt sich den Token vom STS
        /// </summary>
        /// <returns></returns>
        private string GetAuthenticationToken()
        {
            var token = string.Empty;

            if (string.IsNullOrEmpty(TokenURL))
            { 
                throw new Exception("Fehler (BiProBehavior): Für den Bezug eines BiPro-Security Token wird die URL des SecurityToken-Service benötigt. [TokenURL]");
            }

            if (string.IsNullOrEmpty(TokenUsername))
            { 
                throw new Exception("Fehler (BiProBehavior): Für den Bezug eines BiPro-Security Token wird der Benutzername für den SecurityToken-Service benötigt. [TokenUsername]");
            }

            if (string.IsNullOrEmpty(TokenPassword))
            { 
                throw new Exception("Fehler (BiProBehavior): Für den Bezug eines BiPro-Security Token wird das Passwort für den SecurityToken-Service benötigt. [TokenPassword]");
            }

            //  erstellt Tokenanfrage            
            var tokenrequest = 
                //  Bei Tokenvorlagedatei diese verwenden
                !string.IsNullOrEmpty(TokenTemplatepath) && File.Exists(TokenTemplatepath) ?
                string.Format(
                    File.ReadAllText(TokenTemplatepath),
                    TokenUsername,
                    TokenPassword
                ) :
                //  Ansonsten Standardvorlage für Tokenanfrage
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><env:Envelope xmlns:wst=\"http://schemas.xmlsoap.org/ws/2005/02/trust\" xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><env:Header><Security xmlns=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"><UsernameToken xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">" +
                "<Username>" + TokenUsername + "</Username>" +
                "<Password>" + TokenPassword + "</Password>" +
                "</UsernameToken></Security></env:Header><env:Body><wst:RequestSecurityToken><wst:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</wst:RequestType><wst:TokenType>http://schemas.xmlsoap.org/ws/2005/02/sc/sct</wst:TokenType><all:BiPROVersion xmlns:all=\"http://www.bipro.net/namespace/allgemein\">2.1.3.1.0</all:BiPROVersion></wst:RequestSecurityToken></env:Body></env:Envelope>"
            ;

            //  holt das SecurityToken 
            var hinweise = string.Empty;
            var doc = new XDocument();
            var tokenresponse = GetResponse(TokenURL, tokenrequest, ref hinweise);

            if (!tokenresponse.Equals(string.Empty))
            {
                //  Prüfung nach Antwort
                doc = XDocument.Parse(tokenresponse);

                XNamespace sc = "http://schemas.xmlsoap.org/ws/2005/02/sc";

                var get = doc.Descendants(sc + "Identifier").FirstOrDefault();
                token = get == null ? string.Empty : get.Value;
            }

            if (!string.IsNullOrEmpty(Tokenpath) && !string.IsNullOrEmpty(token))
            {
                try
                {
                    File.WriteAllText(Tokenpath, token);
                }
                catch(IOException ex)
                {
                    throw new IOException(string.Format(
                        "BiPRO-Behavior: Das abgefragte BiPRO-Token konnte nicht unter '{0}' gespeichert werden. ({1})",
                        Tokenpath,
                        ex.Message
                    ));
                }
            }

            return token;
        }
        /// <summary>
        /// Führt die Anfrage unter der angegebenen URL aus
        /// </summary>
        /// <param name="sUrl">Anfrage-URL</param>
        /// <param name="sXML">Anfrage</param>
        /// <returns>Ergebnis</returns>
        private string GetResponse(string sUrl, string sXML, ref string Hinweise)
        {
            var webRequest = default(HttpWebRequest);
            var response = string.Empty;

            var encoding = Encoding.UTF8;

            //Anfrage an den Webservice absenden
            var buffer = encoding.GetBytes(sXML);
            webRequest = (HttpWebRequest)HttpWebRequest.Create(sUrl);
            webRequest.Method = "POST";
            webRequest.ServicePoint.Expect100Continue = true;
            webRequest.Timeout = 120000;

            //  Wichtig für den Aufruf, da ansosten immer "Action ist nicht vorhanden!" von der gegenstelle zurück kommt.
            webRequest.Headers.Add("SOAPAction", "\"urn:RequestSecurityToken\"");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";

            /*
             * Ohne dies bekommt man auf dem Server die folgende Fehlermeldung:
             * The underlying connection was closed: Could not establish trust relationship for the SSL/TLS 
             * secure channel. -->The remote certificate is invalid according to the validation procedure.
            */
            ServicePointManager.ServerCertificateValidationCallback =
                delegate
                {
                    return true;
                }
            ;

            using (var writeStream = new StreamWriter(webRequest.GetRequestStream()))
            {
                writeStream.Write(sXML, 0, buffer.Length);
            }
            try
            {
                using (var tokenresponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    if (tokenresponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (var stream = tokenresponse.GetResponseStream())
                        {
                            using (var reader = new StreamReader(stream, encoding))
                            {
                                response = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            finally { }

            return response;
        }
    }
}
