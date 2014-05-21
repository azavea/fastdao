@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem Assumes it is being run in the trunk dir.
rem %1 is a version number.

if "%1" equ "" (
    set version=Dev
) else (
    set version=%1
)

rem Optionally %2 is the trunk directory.
if "%2" neq "" (
    pushd "%2"
)

if exist apidocs.shfb del apidocs.shfb

echo ^<project schemaVersion="1.6.0.7"^> >>apidocs.shfb
echo     ^<assemblies^> >> apidocs.shfb
echo         ^<assembly assemblyPath=".\Azavea.Open.Common\bin\Release\Azavea.Open.Common.dll" xmlCommentsPath=".\Azavea.Open.Common\bin\Release\Azavea.Open.Common.XML" commentsOnly="False" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath="" xmlCommentsPath=".\Azavea.Open.Common\Azavea.Open.Common.NamespaceDocs.xml" commentsOnly="True" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath=".\Azavea.Open.DAO\bin\Release\Azavea.Open.DAO.dll" xmlCommentsPath=".\Azavea.Open.DAO\bin\Release\Azavea.Open.DAO.XML" commentsOnly="False" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath="" xmlCommentsPath=".\Azavea.Open.DAO\Azavea.Open.DAO.NamespaceDocs.xml" commentsOnly="True" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath=".\Azavea.Open.DAO.CSV\bin\Release\Azavea.Open.DAO.CSV.dll" xmlCommentsPath=".\Azavea.Open.DAO.CSV\bin\Release\Azavea.Open.DAO.CSV.XML" commentsOnly="False" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath="" xmlCommentsPath=".\Azavea.Open.DAO.CSV\Azavea.Open.DAO.CSV.NamespaceDocs.xml" commentsOnly="True" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath=".\Azavea.Open.DAO.Firebird\bin\Release\Azavea.Open.DAO.Firebird.dll" xmlCommentsPath=".\Azavea.Open.DAO.Firebird\bin\Release\Azavea.Open.DAO.Firebird.XML" commentsOnly="False" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath="" xmlCommentsPath=".\Azavea.Open.DAO.Firebird\Azavea.Open.DAO.Firebird.NamespaceDocs.xml" commentsOnly="True" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath=".\Azavea.Open.DAO.OleDb\bin\Release\Azavea.Open.DAO.OleDb.dll" xmlCommentsPath=".\Azavea.Open.DAO.OleDb\bin\Release\Azavea.Open.DAO.OleDb.XML" commentsOnly="False" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath="" xmlCommentsPath=".\Azavea.Open.DAO.OleDb\Azavea.Open.DAO.OleDb.NamespaceDocs.xml" commentsOnly="True" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath=".\Azavea.Open.DAO.PostgreSQL\bin\Release\Azavea.Open.DAO.PostgreSQL.dll" xmlCommentsPath=".\Azavea.Open.DAO.PostgreSQL\bin\Release\Azavea.Open.DAO.PostgreSQL.XML" commentsOnly="False" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath="" xmlCommentsPath=".\Azavea.Open.DAO.PostgreSQL\Azavea.Open.DAO.PostgreSQL.NamespaceDocs.xml" commentsOnly="True" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath=".\Azavea.Open.DAO.SQLite\bin\Release\Azavea.Open.DAO.SQLite.dll" xmlCommentsPath=".\Azavea.Open.DAO.SQLite\bin\Release\Azavea.Open.DAO.SQLite.XML" commentsOnly="False" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath="" xmlCommentsPath=".\Azavea.Open.DAO.SQLite\Azavea.Open.DAO.SQLite.NamespaceDocs.xml" commentsOnly="True" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath=".\Azavea.Open.DAO.SQLServer\bin\Release\Azavea.Open.DAO.SQLServer.dll" xmlCommentsPath=".\Azavea.Open.DAO.SQLServer\bin\Release\Azavea.Open.DAO.SQLServer.XML" commentsOnly="False" /^> >> apidocs.shfb
echo         ^<assembly assemblyPath="" xmlCommentsPath=".\Azavea.Open.DAO.SQLServer\Azavea.Open.DAO.SQLServer.NamespaceDocs.xml" commentsOnly="True" /^> >> apidocs.shfb
echo     ^</assemblies^> >> apidocs.shfb
echo     ^<dependencies^> >> apidocs.shfb
echo         ^<dependencyItem depPath=".\Azavea.Open.Common\lib\log4net\log4net.dll" /^> >> apidocs.shfb
echo         ^<dependencyItem depPath=".\Azavea.Open.Common\lib\nunit\nunit.framework.dll" /^> >> apidocs.shfb
echo         ^<dependencyItem depPath=".\Azavea.Open.DAO\lib\geoapi\GeoAPI.dll" /^> >> apidocs.shfb
echo         ^<dependencyItem depPath=".\Azavea.Open.DAO.PostgreSQL\lib\nettopologysuite\NetTopologySuite.dll" /^> >> apidocs.shfb
echo     ^</dependencies^> >> apidocs.shfb
echo     ^<ProjectSummary^> FastDAO is a data-access library intended to simplify access to different databases and other data sources. >> apidocs.shfb
echo                        See the ^&lt;a href="http://sourceforge.net/apps/mediawiki/fastdao/index.php?title=Main_Page"^&gt;wiki^&lt;/a^&gt; for an introduction and tutorials. >> apidocs.shfb
echo                        ^</ProjectSummary^> >> apidocs.shfb
echo     ^<MissingTags^>Summary, Parameter, Returns, AutoDocumentCtors, Namespace, TypeParameter^</MissingTags^> >> apidocs.shfb
echo     ^<VisibleItems^>ExplicitInterfaceImplementations, InheritedMembers, InheritedFrameworkMembers, Protected, SealedProtected^</VisibleItems^> >> apidocs.shfb
echo     ^<HtmlHelp1xCompilerPath path="" /^> >> apidocs.shfb
echo     ^<HtmlHelp2xCompilerPath path="" /^> >> apidocs.shfb
echo     ^<OutputPath^>.^\new_apidocs^\^</OutputPath^> >> apidocs.shfb
echo     ^<SandcastlePath path="" /^> >> apidocs.shfb
echo     ^<WorkingPath path="" /^> >> apidocs.shfb
echo     ^<CleanIntermediates^>True^</CleanIntermediates^> >> apidocs.shfb
echo     ^<KeepLogFile^>False^</KeepLogFile^> >> apidocs.shfb
echo     ^<BuildLogFile path="" /^> >> apidocs.shfb
echo     ^<HelpFileFormat^>Website^</HelpFileFormat^> >> apidocs.shfb
echo     ^<CppCommentsFixup^>False^</CppCommentsFixup^> >> apidocs.shfb
echo     ^<FrameworkVersion^>2.0.50727^</FrameworkVersion^> >> apidocs.shfb
echo     ^<IndentHtml^>False^</IndentHtml^> >> apidocs.shfb
echo     ^<Preliminary^>False^</Preliminary^> >> apidocs.shfb
echo     ^<RootNamespaceContainer^>True^</RootNamespaceContainer^> >> apidocs.shfb
echo     ^<RootNamespaceTitle^>FastDAO v!version!^</RootNamespaceTitle^> >> apidocs.shfb

rem Get the date so we can put it in the docs.
FOR /F "tokens=*" %%A IN ('DATE/T') DO FOR %%B IN (%%A) DO SET Today=%%B

echo     ^<HelpTitle^>FastDAO: The fast, easy way of converting data to objects.  You're looking at the API docs for v!version!, generated !Today!.^</HelpTitle^> >> apidocs.shfb
echo     ^<HtmlHelpName^>Documentation^</HtmlHelpName^> >> apidocs.shfb
echo     ^<Language^>en-US^</Language^> >> apidocs.shfb
echo     ^<CopyrightHref /^> >> apidocs.shfb
echo     ^<CopyrightText /^> >> apidocs.shfb
echo     ^<FeedbackEMailAddress /^> >> apidocs.shfb
echo     ^<FeedbackEMailLinkText /^> >> apidocs.shfb
echo     ^<HeaderText /^> >> apidocs.shfb
echo     ^<FooterText^>Powered by ^&lt;a href="http://sourceforge.net/projects/fastdao" target="_parent"^&gt;^&lt;img style="border:none" src="http://sflogo.sourceforge.net/sflogo.php?group_id=275102^&amp;amp;type=10" width="80" height="15" alt="Get FastDAO at SourceForge.net. Fast, secure and Free Open Source software downloads" /^&gt;^&lt;/a^&gt;  ^&lt;a href="http://fastdao.sourceforge.net" target="_parent"^&gt;Return to project home page.^&lt;/a^&gt;^</FooterText^> >> apidocs.shfb
echo     ^<ProjectLinkType^>Local^</ProjectLinkType^> >> apidocs.shfb
echo     ^<SdkLinkType^>Index^</SdkLinkType^> >> apidocs.shfb
echo     ^<SdkLinkTarget^>Blank^</SdkLinkTarget^> >> apidocs.shfb
echo     ^<PresentationStyle^>hana^</PresentationStyle^> >> apidocs.shfb
echo     ^<NamingMethod^>MemberName^</NamingMethod^> >> apidocs.shfb
echo     ^<SyntaxFilters^>Standard^</SyntaxFilters^> >> apidocs.shfb
echo     ^<ShowFeedbackControl^>False^</ShowFeedbackControl^> >> apidocs.shfb
echo     ^<BinaryTOC^>True^</BinaryTOC^> >> apidocs.shfb
echo     ^<IncludeFavorites^>False^</IncludeFavorites^> >> apidocs.shfb
echo     ^<CollectionTocStyle^>Hierarchical^</CollectionTocStyle^> >> apidocs.shfb
echo     ^<IncludeStopWordList^>True^</IncludeStopWordList^> >> apidocs.shfb
echo     ^<PlugInNamespaces^>ms.vsipcc+, ms.vsexpresscc+^</PlugInNamespaces^> >> apidocs.shfb
echo     ^<HelpFileVersion^>1.0.0.0^</HelpFileVersion^> >> apidocs.shfb
echo     ^<ContentPlacement^>AboveNamespaces^</ContentPlacement^> >> apidocs.shfb
echo ^</project^> >> apidocs.shfb

"c:\Program Files\EWSoftware\Sandcastle Help File Builder\SandcastleBuilderConsole.exe" apidocs.shfb
if not %errorlevel% equ 0 (
    echo Error: Error while running sandcastle.
    exit /b 1
)
echo Removing really old API docs if they exist...
if exist old_apidocs (
    rd /s /q old_apidocs
    if %errorlevel% neq 0 (
        echo Error: Unable to remove old docs dir.
        exit /b 2
    )
)

if exist new_apidocs (
    echo Moving old API docs if they exist...
    if exist api_!version! (
        move api_!version! old_apidocs
        if %errorlevel% neq 0 (
            echo Error: Unable to rename current docs dir to old.
            exit /b 3
        )
    )
    echo Moving new API docs to correct directory name...
    move new_apidocs api_!version!
    if %errorlevel% neq 0 (
        echo Error: Unable to rename new docs dir to current.
        exit /b 4
    )
) else (
    echo Error: New docs dir not found.
    exit /b 5
)

if "%2" neq "" (
    popd
)
