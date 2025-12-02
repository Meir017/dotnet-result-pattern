; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
RN001 | ResultNet.Usage | Error | Do not return null from Result-returning methods
RN002 | ResultNet.Usage | Error | Do not use default with Result types
RN003 | ResultNet.Usage | Warning | Do not use null-coalescing operator with Result types
RN004 | ResultNet.Usage | Warning | Do not check Result types for null
RN005 | ResultNet.Usage | Error | Do not assign null to Result variables
RN006 | ResultNet.Usage | Warning | Do not use null-coalescing assignment with Result types
RN007 | ResultNet.Usage | Warning | Do not use null-conditional operators with Result types
RN008 | ResultNet.Usage | Warning | Do not use null-forgiving operator with Result types
RN009 | ResultNet.Usage | Warning | Do not check for null in switch expressions with Result types
RN010 | ResultNet.Usage | Error | Do not use null as default value for Result parameters
