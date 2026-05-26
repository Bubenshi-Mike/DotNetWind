namespace DotNetWind.Templates;

public static class TailwindCssTemplate
{
    public static string GetDefault() =>
        """
        @import "tailwindcss";

        @theme {
            --font-sans: 'Arimo', 'Geist', ui-sans-serif, system-ui, sans-serif;
            --font-mono: 'Geist Mono', ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
        }

        @layer base {
            *,
            *::before,
            *::after {
                box-sizing: border-box;
            }

            html {
                font-family: var(--font-sans);
                font-size: 0.875rem;
                line-height: 1.5;
                -webkit-font-smoothing: antialiased;
                -moz-osx-font-smoothing: grayscale;
                text-rendering: optimizeLegibility;
                scroll-behavior: smooth;
            }

            body {
                font-family: inherit;
                font-size: 0.875rem;
                font-weight: 400;
                line-height: inherit;
                min-height: 100vh;
                margin: 0;
            }

            :focus {
                outline: none;
            }

            :focus-visible {
                outline: 2px solid oklch(70% 0.15 250);
                outline-offset: 2px;
            }
        }
        """;
}
