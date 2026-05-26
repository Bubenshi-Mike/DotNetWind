namespace DotNetWind.Templates;

public static class MsBuildTargetTemplate
{
    public const string TargetName = "BuildTailwind";

    public static string GetTarget() =>
        """
          <Target Name="BuildTailwind" BeforeTargets="Build">
            <Message Text="Building Tailwind CSS..." Importance="high" />

            <Exec Command="npm run tw:build"
                  Condition="'$(Configuration)' == 'Debug'" />

            <Exec Command="npm run tw:build:min"
                  Condition="'$(Configuration)' == 'Release'" />
          </Target>
        """;
}
