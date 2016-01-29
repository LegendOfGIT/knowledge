using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Allgemeine Informationen über eine Assembly werden über die folgenden 
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die mit einer Assembly verknüpft sind.
[assembly: AssemblyTitle("FB.Xml.XPath.Extensions")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Franke und Bornberg Research GmbH")]
[assembly: AssemblyProduct("FB.Xml.XPath.Extensions")]
[assembly: AssemblyCopyright("Copyright ©  2014-2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Durch Festlegen von ComVisible auf "false" werden die Typen in dieser Assembly unsichtbar 
// für COM-Komponenten.  Wenn Sie auf einen Typ in dieser Assembly von 
// COM zugreifen müssen, legen Sie das ComVisible-Attribut für diesen Typ auf "true" fest.
[assembly: ComVisible(false)]

// Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird
[assembly: Guid("4681317e-b563-418e-a965-0c30b93ee769")]

//
// CA1014 Assemblys mit CLSCompliantAttribute markieren, 
// da so extern sichtbare Typen verfügbar gemacht werden.
//
[assembly: System.CLSCompliant(true)]

//
// CA1824 Assemblys mit NeutralResourcesLanguageAttribute markieren	
// und geben Sie die Sprache der Ressourcen innerhalb der Assembly an. 
// Möglicherweise wird so die Suchleistung beim ersten Abrufen einer 
// Ressource gesteigert.
//
[assembly: System.Resources.NeutralResourcesLanguageAttribute("de-DE")]

#region Version
// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
// Hauptversion.Nebenversion.Buildnummer.Revision
//
[assembly: AssemblyVersion(V.AssemblyVersion)]
//
// Hauptversion.Nebenversion.Buildnummer.Revision
//
[assembly: AssemblyFileVersion(V.AssemblyVersion)]
//
// // Hauptversion.Nebenversion.Buildnummer-[alpha-Revision|beta-Revision|rc-Revision]
//
[assembly: AssemblyInformationalVersion(V.InfoVersion)]

/// <summary>                                                       
/// Version Infomation
/// </summary>
internal struct V
{
    /// <summary>
    /// Version mit drei Stellen.
    /// </summary>
    internal const string Version = "1.0.1";

    /// <summary>
    /// Vierstellige Version da es im Attribut sonst einen Compiler-Fehler gibt.
    /// </summary>
    internal const string AssemblyVersion = Version + ".0";

    /// <summary>
    /// Der Zusatz zur Info z.B. -alpha.1, -beta.4 oder -rc.12.
    /// </summary>
    internal const string State = "";

    /// <summary>
    /// Die lesbare Version nach Standard http://semver.org/.
    /// </summary>
    internal const string InfoVersion = Version + State;
}
#endregion