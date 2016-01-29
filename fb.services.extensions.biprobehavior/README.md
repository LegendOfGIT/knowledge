# fb.services.extensions.biprobehavior

## <a>Inhalt</a>
1. [Was tut dieses Behavior?](#purpose "Was tut dieses Behavior?")
1. [Wo wird das Behavior eingesetzt?](#location "Wo wird das Behavior eingesetzt?")
1. [Voraussetzungen](#requirements "Voraussetzungen")
1. [Wie wird dieses Behavior installiert?](#installation "Wie wird dieses Behavior installiert?")
1. [Wie wird das Behavior für einen Endpunkt eingerichtet?](#endpointconfiguration "Wie wird das Behavior für einen Endpunkt eingerichtet?")
1. [Wie kann das Behavior konfiguriert werden?](#configuration "Wie kann das Behavior konfiguriert werden?")

### <a name='purpose'>Was tut dieses Behavior?</a>
Dieses Behavior kann über die Web.-/App.config eines Programm/Webservice für einen Endpunkt konfiguriert werden.
Ist das Behavior aktiv, wird vor jedem Absenden eines Request an den konfigurierten Endpunkt der Header der abgehenden
Soap-Nachricht überschrieben und ein BiPRO SecurityToken in dem Header eingefügt.
Dieses wird für die zur Zeit übliche Form der BiPRO-Authentifizierung benötigt.
Das BiPRO Security-Token wird hierbei von dem Behavior über einen für das Behavior konfigurierten BiPRO Token Webservice angefragt.

Nach der Modifikation des BiProBehavior, sieht der Soap-Header in etwa so aus:
<Header>
  <Security xmlns="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
    <SecurityContextToken xmlns="http://schemas.xmlsoap.org/ws/2005/02/sc">
      <Identifier>bipro:MTQ1MzQ0OTE5Mzg3MTE0NTM0NDkxOTM4NzExNDUzND...</Identifier>
    </SecurityContextToken>
  </Security>
</Header>

### <a name='location'>Wo wird das Behavior eingesetzt?</a>
Das Behavior kann in jedem .NET Projekt - zum Beispiel in Webservices oder Konsolenanwendungen - eingesetzt werden.

### <a name='requirements'>Voraussetzungen</a>
Für den Einsatz wird mindestens das .NET-Framework 4.0 benötigt, da innerhalb des Behaviorcode das Element Tuple<T, T> eingesetzt wird.
Des Weiteren muss das jeweilige Programm in einem .NET Projekt liegen, damit das Bevaior als Package installiert werden kann.

### <a name='installation'>Wie wird dieses Behavior installiert?</a>
1. Zu erweiterndes Projekt öffnen
1. Paket-Manager-Konsole öffnen
1. Befehl 'install-package fb.services.extensions.behaviors.biprobehavior' ausführen

### <a name='endpointconfiguration'>Wie wird das Behavior für einen Endpunkt eingerichtet?</a>
Als erstes wird die Behavior-Extension innerhalb der App-/Web.Config bekannt gemacht.
Hierfür wird folgender Konfigurationsabschnitt im Abschnitt "system.serviceModel" 
zwischen den Unterabschnitten "behaviors" und "bindings" platziert:

``` xml
<configuration>
  ...
  <system.serviceModel>
    ...
    <behaviors>...</behaviors>
    <extensions>
      <behaviorExtensions>
        <add 
          name="BiProBehavior" 
**          type="FB.Services.Extensions.Behaviors.BiProBehavior, FB.Services.Extensions.Behaviors.BiProBehavior, Version=1.0.7.0"**
        />
      </behaviorExtensions>
    </extensions>
    <bindings>...</bindings>
  </system.serviceModel>
  ...
</configuration>	  
```

Ist die Behavior-Extension bekannt, kann eine Behavior Abschnitt für die Behavior-Extension hinzukonfiguriert werden:
``` xml
<configuration>
  ...
  <system.serviceModel>
    <behaviors>
      <endpointBehaviors>
      <behavior name="BiPROBehavior">
**        <BiProBehavior**
          TokenURL="https://ws0.barmenia24.de/ibis/services/UsernamePasswordLogin_2.1.3.1.0" 
          TokenUsername="frankebornberg" 
          TokenPassword="fh6vcp0fg" 
**        />**
      </behavior>
      </endpointBehaviors>
    </behaviors>
    <extensions>...</extensions>
    ...
  </system.serviceModel>
  ...
<configuration>
```

Um das konfigurierte Behavior letzendlich bei der Ansprache eines Endpunkt auszulösen, 
muss dem Endpunkt das konfigurierte Behavior zugeordnet werden:
``` xml
<configuration>
  <system.serviceModel>
    ...  
    <behaviors>...</behaviors>
    <extensions>...</extensions>
    <client>
      <endpoint
        address="http://83.133.158.53/BBVN_RC/tarife.asmx"
        binding="basicHttpBinding"
        bindingConfiguration="RechenkernDieBayerischeSoap"
        contract="BBVN.RechenkernDieBayerischeSoap"
        name="RechenkernDieBayerischeSoap"
**        behaviorConfiguration="BiPROBehavior"**
      />
    </client>
    ...  
  </system.serviceModel>
</configuration>
```

### <a name='configuration'>Wie kann das Behavior konfiguriert werden?</a>
**TokenURL / TokenUsername / TokenPassword**
Diese Attribute sind bei der Erstellung einer Konfiguration für die Behavior-Extension immer Pflicht.
Mit diesen Parametern werden die URL und die Zugangsdaten des BiPro SecurityToken Webservice festgelegt, 
mit dem das BiPRO Security-Token für den Soap-Header geholt wird.

#### UseToken
Insofern das Attribut "UseToken" nicht angeben, oder mit "true" belegt wurde, holt die BehaviorExtension
bei jeder Anfrage an den Endpunkt ein BiPRO-Security Token von dem konfigurierten BiPro Security-Token Service
und ersetzt den bestehenden Header mit einem Header der den geholten BiPRO-Security Token enthält.

Wird das Attribut "UseToken" mit "false" konfiguriert, bleibt der Soap-Header wie er ist.
In diesem Fall werden nur die anderen Soap Manipulationen wie das Aufräumen von Namespaces durchgeführt.

#### TokenPath
Mit dem Attribut "TokenPath" kann der Entwickler einen Dateipfad angeben, in dem ein, über den
BiPro Security-Token Service bezogenes BiPRO-Security Token zwischengespeichert werden kann.
Ist der Dateipfad (z.B. c:\temp\temptoken.txt) konfiguriert, wird ein bezogenes BiPRO-Security Token aus der zwischengespeicherten Datei
wiederverwendet, wenn dieses nicht älter als zwei Minuten ist.
Sind die zwei Minuten überschritten, fordert die Behavior-Extension ein neues Token an und speicher dieses zwischen.

**Achtung**
Für das konfigurierte Verzeichnis, müssen natürlich Lese- und Schreibberechtigungen gesetzt sein.

#### TokenTemplatepath
Mit diesem Attribut kann man eine Vorlagendatei für die Anfrage eines BiPro SecurityToken über den BiPro SecurityToken Webservice festlegen.
Bei der autmatischen Anfrage BiPro SecurityToken, wird dann die Soap-Anfrage aus der Vorlagendatei verwendet.
Folgende Ausdrücke, werden in der Vorlage dabei bei einer Anfrage automatisch ersetzt:
{0} = TokenUsername
{1} = TokenPassword

#### Namespaces
Über das Attribut "Namespaces" können mit einem | getrennt mehrere Pärchen 
in der Form "{Namespacepräfix};{Namespaceurl}" angegeben werden.
Bei der Anfrage an den Endpunkt wird dann jeder der festgelegten Namespaces mit dem jeweils festgelegten Präfix
in den Wurzelknoten des Body-Teil gelegt und anschließend alle separaten Definitionen des Namespace
in den XML-Knoten des Bodys entfernt und durch das vorgegebene Präfix ersetzt.

**Ein Beispiel**
Konfiguration 
"barmdaten|http://www.barmenia.de/namespace/datentypen;daten|http://www.bipro.net/namespace/datentypen;partner|http://www.bipro.net/namespace/partner;leben"

**Soap-Body vor der Manipulation**
``` xml
<s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <getQuote xmlns="http://www.bipro.net/namespace/tarifierung">
    <Request xmlns:ns0="http://www.bipro.net/namespace/allgemein">
      <ns0:BiPROVersion>2.1.5.1.2</ns0:BiPROVersion>
      <Tarifierung>
        <Partner xmlns:ns1="http://www.bipro.net/namespace/partner" xsi:type="ns1:CT_Person">
          <ns0:Erweiterung xmlns:ns2="http://www.barmenia.de/namespace/leben" xsi:type="ns2:CT_PersonErweiterung">
            <ns2:IstPolitischExponiertePerson>false</ns2:IstPolitischExponiertePerson>
          </ns0:Erweiterung>
          <ns1:PartnerID>Versicherungsnehmer</ns1:PartnerID>
          <ns1:Anschrift/>
          <ns1:Geburtsdatum>1986-02-24</ns1:Geburtsdatum>
          <ns1:Geschlecht>1</ns1:Geschlecht>
          <ns1:Berufstaetigkeit>
            <ns1:Beruf>Bankkaufmann/-kauffrau</ns1:Beruf>
            <ns1:StellungImBeruf xmlns:ns3="http://www.barmenia.de/namespace/datentypen" xsi:type="ns3:STE_StellungImBerufErweitert">11</ns3:StellungImBeruf>
            <ns1:Berufsschluessel>691009</ns1:Berufsschluessel>
          </ns1:Berufstaetigkeit>
        </Partner>
        ...
      </Tarifierung>
    </Request>
  </getQuote>
</s:Body>
```
		  
**Soap-Body nach der Manipulation**
``` xml
<s:Body xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <getQuote xmlns="http://www.bipro.net/namespace/tarifierung" xmlns:barmdaten="http://www.barmenia.de/namespace/datentypen" xmlns:daten="http://www.bipro.net/namespace/datentypen" xmlns:partner="http://www.bipro.net/namespace/partner" xmlns:leben="http://www.bipro.net/namespace/leben" xmlns:barmleben="http://www.barmenia.de/namespace/leben" xmlns:allgemein="http://www.bipro.net/namespace/allgemein" xmlns:barmtransfer="http://www.barmenia.de/namespace/transfer">
    <Request>
      <allgemein:BiPROVersion>2.1.5.1.2</allgemein:BiPROVersion>
      <Tarifierung>
        <Partner xsi:type="partner:CT_Person">
          <allgemein:Erweiterung xsi:type="barmleben:CT_PersonErweiterung">
            <barmleben:IstPolitischExponiertePerson>false</barmleben:IstPolitischExponiertePerson>
          </allgemein:Erweiterung>
          <partner:PartnerID>Versicherungsnehmer</partner:PartnerID>
          <partner:Anschrift/>
          <partner:Geburtsdatum>1986-02-24</partner:Geburtsdatum>
          <partner:Geschlecht>1</partner:Geschlecht>
          <partner:Berufstaetigkeit>
            <partner:Beruf>Bankkaufmann/-kauffrau</partner:Beruf>
            <partner:StellungImBeruf xsi:type="barmdaten:STE_StellungImBerufErweitert">11</partner:StellungImBeruf>
            <partner:Berufsschluessel>691009</partner:Berufsschluessel>
          </partner:Berufstaetigkeit>
        </Partner>
        ...
      </Tarifierung>
    </Request>
  </getQuote>
</s:Body>		  
```

#### RepairAttributePrefixes
Wenn das Attribut "RepairAttributePrefixes" mit "true" konfiguriert wird, werden alle Werte in allen Xml-Attributen
mit dem Namespacepräfix des Elternelement versehen, wenn die Attributwerte vorher keinen eigenen Namespace verwendet haben.

**Ein Beispiel**

**Vor der Manipulation**
``` xml
<Partner xsi:type="partner:CT_Person">
  <partner:Anschrift>
    <partner:Strasse beispiel="wert"></partner:Strasse>
  </partner:Anschrift>
</Partner>
```

**Nach der Manipulation**
``` xml
<Partner xsi:type="partner:CT_Person">
  <partner:Anschrift>
    <partner:Strasse beispiel="partner:wert"></partner:Strasse>
  </partner:Anschrift>
</Partner>
```

#### NuernbergerBiPROWorkaround
Wenn das Attribut "NuernbergerBiPROWorkaround" mit "true" konfiguriert wird, 
wird in der Anfrage die Information "<.*?ArtID.*?VorgabeBerechnungErweitert.*?</.*?ArtID>"
analysiert und bei bestimmten Vorgabearten der Namespacepräfix der ArtID angepasst.


