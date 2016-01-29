# fb.xml.xpath.extensions

## <a>Inhalt</a>
1. [Was tut dieses Paket?](#purpose "Was tut dieses Paket?")
1. [Wo wird das Paket eingesetzt?](#where "Wo wird das Paket eingesetzt?")
1. [Voraussetzungen](#requirements "Voraussetzungen")
1. [Wie wird dieses Programm installiert?](#installation "Wie wird dieses Programm installiert?")
1. [Verwendungsbeispiel](#sample "Verwendungsbeispiel")

### <a name='purpose'>Was tut dieses Paket?</a>
Dieses Paket stellt Erweiterungen für die Arbeit mit XML XPathes zur Verfügung.
Das Paket wurde ursprünglich erstellt um die automatische Erstellung von XML-Strukturen anhand eines vorgegebenen XPath
in mehreren Anwendungen zur Verfügung zu stellen.

### <a name='where'>Wo wird das Paket eingesetzt?</a>
Die Funktionen des Paket werden ursprünglich und Primär im TarifMasken-Service verwendet.

### <a name='requirements'>Voraussetzungen</a>
- .NET-Framework 3.5 oder höher

### <a name='installation'>Wie wird dieses Paket installiert?</a>
1. Das gewünschte Projekt öffnen
1. Paket-Manager-Konsole öffnen
1. Paketquelle "Franke und Bornberg Repository" wählen
1. Installation des Paket mit der Konsolenzeile 
```
install-package fb.xml.xpath.extensions
```
starten

### <a name='sample'>Verwendungsbeispiel</a>
Ein mögliches Szenario für dieses Paket, könnte wie folgt aussehen:

```
using FB.Xml.XPath.Extensions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var document = new System.Xml.Linq.XDocument();

            var root = new System.Xml.Linq.XElement("root");

            root.XPathAppendElement("//subtest[@id='test 1']/element[@test='testvalue']/test");
            root.XPathAppendElement("//subtest[@id='test 2']/element[@test='testvalue']/test");
            root.XPathAppendElement("//subtest[@id='test 2']/element[@test='testvalue 2']/test");

            document.Add(root);

            document.Save(@"c:\temp\test.xml");
        }
    }
}
```

Das darauf generierte XML hat dann folgenden Aufbau:

``` xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <subtest id="test 1">
    <element test="testvalue">
      <test></test>
    </element>
  </subtest>
  <subtest id="test 2">
    <element test="testvalue">
      <test></test>
    </element>
    <element test="testvalue 2">
      <test></test>
    </element>
  </subtest>
</root>
```