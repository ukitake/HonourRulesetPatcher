using LSLib.LS;
using LSLib.LS.Enums;
using System.Xml.Linq;

Console.WriteLine("---------------------------------");
Console.WriteLine("Honour Ruleset Patcher by ukitake");
Console.WriteLine("https://github.com/ukitake");
Console.WriteLine("---------------------------------\n");
if (args.Length == 0)
{
    Console.WriteLine("ERROR: No save path provided. Full save folder path should be provided as 1st argument.\nPress any key to exit...");
    Console.ReadKey();
    Environment.Exit(-1);
}
Console.WriteLine(args[0]);
var fullSavePath = args[0];

var tempSavePath = $"{Directory.GetCurrentDirectory()}\\NewSave";

var Cleanup = () =>
{
    if (Directory.Exists(tempSavePath))
    {
        Directory.Delete(tempSavePath, true);
    }
};

var Error = (string msg) =>
{
    Cleanup();
    Console.WriteLine(msg);
    Console.ReadKey();
    Environment.Exit(-1);
};

var filesInSaveDir = Directory.GetFiles(fullSavePath);
var saveFile = filesInSaveDir.FirstOrDefault(f => f.EndsWith(".lsv"));
if (saveFile == null)
{
    Error("ERROR: No LSV File in provided directory.\nPress any key to exit...");
    return;
}

try
{
    var packager = new Packager();
    packager.UncompressPackage(saveFile, tempSavePath);
}
catch (NotAPackageException)
{
    Cleanup();
    if (ModPathVisitor.archivePartRe.IsMatch(Path.GetFileName(saveFile)))
    {
        Error($"ERROR: The specified file is part of a multi-part package; only the first part needs to be extracted.\nPress any key to exit...");
        return;
    }
    else
    {
        Error($"ERROR: The specified file ({saveFile}) is not an PAK package or savegame archive.\nPress any key to exit...");
        return;
    }
}

var newSaveFiles = Directory.GetFiles(tempSavePath);
var metaLSF = newSaveFiles.SingleOrDefault(f => f.EndsWith("meta.lsf"));
var globalsLSF = newSaveFiles.SingleOrDefault(f => f.EndsWith("Globals.lsf"));
if (metaLSF == null)
{
    Error("ERROR: No meta.lsf file in extracted save package\nPress any key to exit...");
    return;
}
if (globalsLSF == null)
{
    Error("ERROR: No Globals.lsf in extracted save package\nPress any key to exit...");
    return;
}

var convertFile = (string source, string target) =>
{
    var loadParams = ResourceLoadParameters.FromGameVersion(Game.BaldursGate3);
    loadParams.ByteSwapGuids = !false; // !legacyGuids.Checked;
    var _resource = ResourceUtils.LoadResource(source, loadParams);
    ResourceFormat format = ResourceUtils.ExtensionToResourceFormat(target);
    var conversionParams = ResourceConversionParameters.FromGameVersion(Game.BaldursGate3);
    ResourceUtils.SaveResource(_resource, target, format, conversionParams);
};

// convert them to text files
var metaLSX = metaLSF.Replace(".lsf", ".lsx");
var globalsLSX = globalsLSF.Replace(".lsf", ".lsx");
convertFile(metaLSF, metaLSX);
convertFile(globalsLSF, globalsLSX);

var makeHonourNode = () =>
{
    var honourNode = new XElement("node");
    honourNode.SetAttributeValue("id", "ModuleShortDesc");

    var uuidAtt = new XElement("attribute");
    uuidAtt.SetAttributeValue("id", "UUID");
    uuidAtt.SetAttributeValue("type", "guid");
    uuidAtt.SetAttributeValue("value", "b77b6210-ac50-4cb1-a3d5-5702fb9c744c");

    var version64Att = new XElement("attribute");
    version64Att.SetAttributeValue("id", "Version64");
    version64Att.SetAttributeValue("type", "int64");
    version64Att.SetAttributeValue("value", "36028797024738184");

    var md5Att = new XElement("attribute");
    md5Att.SetAttributeValue("id", "MD5");
    md5Att.SetAttributeValue("type", "LSString");
    md5Att.SetAttributeValue("value", "00da1a1b7578924324da3893ab786f32");

    var folderAtt = new XElement("attribute");
    folderAtt.SetAttributeValue("id", "Folder");
    folderAtt.SetAttributeValue("type", "LSString");
    folderAtt.SetAttributeValue("value", "Honour");

    var nameAtt = new XElement("attribute");
    nameAtt.SetAttributeValue("id", "Name");
    nameAtt.SetAttributeValue("type", "LSString");
    nameAtt.SetAttributeValue("value", "Honour");

    var publishAtt = new XElement("attribute");
    publishAtt.SetAttributeValue("id", "PublishHandle");
    publishAtt.SetAttributeValue("type", "uint64");
    publishAtt.SetAttributeValue("value", "0");

    honourNode.Add(uuidAtt);
    honourNode.Add(version64Att);
    honourNode.Add(md5Att);
    honourNode.Add(folderAtt);
    honourNode.Add(nameAtt);
    honourNode.Add(publishAtt);

    return honourNode;
};

// patch the text files
XDocument meta = XDocument.Load($"{tempSavePath}\\meta.lsx");
// find the GustavDev node and add the Honour Mode mod right after it
var gustavDev = meta.DescendantNodes().Where(e => (e as XElement)?.Attribute("value")?.Value == "GustavDev").FirstOrDefault()?.Parent;
if (gustavDev == null)
{
    Error("ERROR: No 'GustavDev' <node> found in meta.lsx file.\nPress any key to exit...");
    return;
}
gustavDev.AddAfterSelf(makeHonourNode());

// find the Standard ruleset entry and change it to Honour ruleset (I think this is what's happening)
var ruleSet = meta.DescendantNodes().Where(e => (e as XElement)?.Attribute("value")?.Value == "3f1cb183-ef6e-4db6-b2ed-a703cb217264").FirstOrDefault();
if (ruleSet == null)
{
    Error("ERROR: No ruleset <node> with uuid '3f1cb183-ef6e-4db6-b2ed-a703cb217264' found in meta.lsx file.\nPress any key to exit...");
    return;
}
(ruleSet as XElement)?.SetAttributeValue("value", "5d595a7a-6182-4559-b404-ed7cc7ad8ada");
meta.Save($"{tempSavePath}\\meta.lsx");

XDocument globals = XDocument.Load($"{tempSavePath}\\Globals.lsx");
var globalsGustavDev = globals.DescendantNodes().Where(e => (e as XElement)?.Attribute("value")?.Value == "GustavDev").FirstOrDefault()?.Parent;
if (globalsGustavDev == null)
{
    Error("ERROR: No 'GustavDev' <node> found in Globals.lsx file.\nPress any key to exit...");
    return;
}
globalsGustavDev.AddAfterSelf(makeHonourNode());
globals.Save($"{tempSavePath}\\Globals.lsx");

// convert them back
convertFile(metaLSX, metaLSF);
convertFile(globalsLSX, globalsLSF);

// delete the text files to prepare the dir for repackaging
File.Delete(metaLSX);
File.Delete(globalsLSX);

try
{
    var build = new PackageBuildData();
    build.Version = PackageVersion.V18; // BG3
    build.Compression = CompressionMethod.Zlib;

    // Fallback to Zlib, if the package version doesn't support LZ4
    if (build.Compression == CompressionMethod.LZ4 && build.Version <= PackageVersion.V9)
    {
        build.Compression = CompressionMethod.Zlib;
    }

    build.Priority = (byte)0;

    var packager = new Packager();
    packager.CreatePackage(saveFile, tempSavePath, build).Wait();

    Console.WriteLine("SUCCESS: Package created successfully!");
}
catch (Exception exc)
{
    Error($"ERROR: Internal error!{Environment.NewLine}{Environment.NewLine}{exc} \nPackage Build Failed\nPress any key to exit...");
    return;
}

Cleanup();
Thread.Sleep(1000);
