using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace BlazorApp.Shared.CodeModels
{
    public partial class GeneratedAssemblyModel
    {
        [JsonProperty("compilation")]
        public Compilation Compilation { get; set; }

        [JsonProperty("assemblyBytes")]
        public string AssemblyBytes { get; set; }

        [JsonProperty("assemblyString")]
        public string AssemblyString { get; set; }

        [JsonProperty("diagnostics")]
        public List<object> Diagnostics { get; set; }
        [JsonIgnore]
        public byte[] AssemblyByteArray { get; set; }
    }

    public partial class Compilation
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("isCaseSensitive")]
        public bool IsCaseSensitive { get; set; }

        [JsonProperty("options")]
        public CompilationOptions Options { get; set; }

        [JsonProperty("languageVersion")]
        public long LanguageVersion { get; set; }

        [JsonProperty("scriptCompilationInfo")]
        public object ScriptCompilationInfo { get; set; }

        [JsonProperty("syntaxTrees")]
        public List<SyntaxTree> SyntaxTrees { get; set; }

        [JsonProperty("directiveReferences")]
        public List<object> DirectiveReferences { get; set; }

        [JsonProperty("referencedAssemblyNames")]
        public List<ReferencedAssemblyName> ReferencedAssemblyNames { get; set; }

        [JsonProperty("assembly")]
        public Assembly Assembly { get; set; }

        [JsonProperty("sourceModule")]
        public Assembly SourceModule { get; set; }

        [JsonProperty("globalNamespace")]
        public Assembly GlobalNamespace { get; set; }

        [JsonProperty("scriptClass")]
        public object ScriptClass { get; set; }

        [JsonProperty("dynamicType")]
        public Assembly DynamicType { get; set; }

        [JsonProperty("objectType")]
        public Assembly ObjectType { get; set; }

        [JsonProperty("assemblyName")]
        public string AssemblyName { get; set; }

        [JsonProperty("externalReferences")]
        public List<Reference> ExternalReferences { get; set; }

        [JsonProperty("references")]
        public List<Reference> References { get; set; }
    }

    public partial class Assembly
    {
    }

    public partial class Reference
    {
        [JsonProperty("display")]
        public string Display { get; set; }

        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("kind")]
        public long Kind { get; set; }

        [JsonProperty("aliases")]
        public List<object> Aliases { get; set; }

        [JsonProperty("embedInteropTypes")]
        public bool EmbedInteropTypes { get; set; }
    }

    public partial class CompilationOptions
    {
        [JsonProperty("allowUnsafe")]
        public bool AllowUnsafe { get; set; }

        [JsonProperty("usings")]
        public List<object> Usings { get; set; }

        [JsonProperty("nullableContextOptions")]
        public long NullableContextOptions { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("outputKind")]
        public long OutputKind { get; set; }

        [JsonProperty("moduleName")]
        public object ModuleName { get; set; }

        [JsonProperty("scriptClassName")]
        public string ScriptClassName { get; set; }

        [JsonProperty("mainTypeName")]
        public object MainTypeName { get; set; }

        [JsonProperty("cryptoPublicKey")]
        public List<object> CryptoPublicKey { get; set; }

        [JsonProperty("cryptoKeyFile")]
        public object CryptoKeyFile { get; set; }

        [JsonProperty("cryptoKeyContainer")]
        public object CryptoKeyContainer { get; set; }

        [JsonProperty("delaySign")]
        public object DelaySign { get; set; }

        [JsonProperty("publicSign")]
        public bool PublicSign { get; set; }

        [JsonProperty("checkOverflow")]
        public bool CheckOverflow { get; set; }

        [JsonProperty("platform")]
        public long Platform { get; set; }

        [JsonProperty("optimizationLevel")]
        public long OptimizationLevel { get; set; }

        [JsonProperty("generalDiagnosticOption")]
        public long GeneralDiagnosticOption { get; set; }

        [JsonProperty("warningLevel")]
        public long WarningLevel { get; set; }

        [JsonProperty("concurrentBuild")]
        public bool ConcurrentBuild { get; set; }

        [JsonProperty("deterministic")]
        public bool Deterministic { get; set; }

        [JsonProperty("metadataImportOptions")]
        public long MetadataImportOptions { get; set; }

        [JsonProperty("specificDiagnosticOptions")]
        public Assembly SpecificDiagnosticOptions { get; set; }

        [JsonProperty("reportSuppressedDiagnostics")]
        public bool ReportSuppressedDiagnostics { get; set; }

        [JsonProperty("metadataReferenceResolver")]
        public object MetadataReferenceResolver { get; set; }

        [JsonProperty("xmlReferenceResolver")]
        public object XmlReferenceResolver { get; set; }

        [JsonProperty("sourceReferenceResolver")]
        public object SourceReferenceResolver { get; set; }

        [JsonProperty("strongNameProvider")]
        public object StrongNameProvider { get; set; }

        [JsonProperty("assemblyIdentityComparer")]
        public Assembly AssemblyIdentityComparer { get; set; }

        [JsonProperty("errors")]
        public List<object> Errors { get; set; }
    }

    public partial class ReferencedAssemblyName
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("cultureName")]
        public string CultureName { get; set; }

        [JsonProperty("flags")]
        public long Flags { get; set; }

        [JsonProperty("contentType")]
        public long ContentType { get; set; }

        [JsonProperty("hasPublicKey")]
        public bool HasPublicKey { get; set; }

        [JsonProperty("publicKey")]
        public List<long> PublicKey { get; set; }

        [JsonProperty("publicKeyToken")]
        public List<long> PublicKeyToken { get; set; }

        [JsonProperty("isStrongName")]
        public bool IsStrongName { get; set; }

        [JsonProperty("isRetargetable")]
        public bool IsRetargetable { get; set; }
    }

    public partial class SyntaxTree
    {
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        [JsonProperty("encoding")]
        public object Encoding { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("hasCompilationUnitRoot")]
        public bool HasCompilationUnitRoot { get; set; }

        [JsonProperty("options")]
        public SyntaxTreeOptions Options { get; set; }

        [JsonProperty("diagnosticOptions")]
        public Assembly DiagnosticOptions { get; set; }
    }

    public partial class SyntaxTreeOptions
    {
        [JsonProperty("languageVersion")]
        public long LanguageVersion { get; set; }

        [JsonProperty("specifiedLanguageVersion")]
        public long SpecifiedLanguageVersion { get; set; }

        [JsonProperty("preprocessorSymbolNames")]
        public List<object> PreprocessorSymbolNames { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("features")]
        public Assembly Features { get; set; }

        [JsonProperty("kind")]
        public long Kind { get; set; }

        [JsonProperty("specifiedKind")]
        public long SpecifiedKind { get; set; }

        [JsonProperty("documentationMode")]
        public long DocumentationMode { get; set; }

        [JsonProperty("errors")]
        public List<object> Errors { get; set; }
    }
}
