using System.Net;
using System.Text;

namespace EutherBoot;

public static class AdminPage
{
    public static string Render(IReadOnlyList<BootProfile> profiles, IReadOnlyList<BootAssignment> assignments, string bootUrl)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html lang=\"sv\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        sb.AppendLine("<title>EutherBoot</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{margin:0;font:15px/1.45 system-ui,sans-serif;background:#f6f7f9;color:#172026}main{max-width:1040px;margin:0 auto;padding:32px 20px}h1{font-size:34px;margin:0 0 4px}h2{font-size:18px;margin:0 0 12px}.muted{color:#66717a}.grid{display:grid;grid-template-columns:1fr 1fr;gap:16px;margin-top:24px}.panel{background:white;border:1px solid #d8dee4;border-radius:8px;padding:18px}table{width:100%;border-collapse:collapse}td,th{text-align:left;border-top:1px solid #e6eaee;padding:9px 4px}code{background:#eef1f4;border-radius:4px;padding:2px 5px}input,select,button{font:inherit}input,select{width:100%;box-sizing:border-box;border:1px solid #c9d1d9;border-radius:6px;padding:8px;margin:4px 0 10px}button{border:0;border-radius:6px;background:#1264a3;color:white;padding:9px 12px;cursor:pointer}.stack{display:grid;gap:10px}@media(max-width:760px){.grid{grid-template-columns:1fr}}");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body><main>");
        sb.AppendLine("<h1>EutherBoot</h1>");
        sb.AppendLine($"<div class=\"muted\">boot-url: <code>{WebUtility.HtmlEncode(bootUrl)}</code></div>");

        sb.AppendLine("<div class=\"grid\">");
        sb.AppendLine("<section class=\"panel\"><h2>Profiler</h2><table><thead><tr><th>Namn</th><th>Typ</th><th>Kernel</th></tr></thead><tbody>");
        foreach (var profile in profiles)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{WebUtility.HtmlEncode(profile.Label)}<br><span class=\"muted\">{WebUtility.HtmlEncode(profile.Name)}</span></td>");
            sb.AppendLine($"<td>{WebUtility.HtmlEncode(profile.Type)}</td>");
            sb.AppendLine($"<td><code>{WebUtility.HtmlEncode(profile.Boot.Kernel)}</code></td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody></table></section>");

        sb.AppendLine("<section class=\"panel\"><h2>MAC-styrning</h2>");
        sb.AppendLine("<form method=\"post\" action=\"/api/assignments\" class=\"stack\">");
        sb.AppendLine("<label>MAC-adress<input name=\"mac\" placeholder=\"52:54:00:12:34:56\"></label>");
        sb.AppendLine("<label>Profil<select name=\"profile\">");
        foreach (var profile in profiles)
            sb.AppendLine($"<option value=\"{WebUtility.HtmlEncode(profile.Name)}\">{WebUtility.HtmlEncode(profile.Label)}</option>");
        sb.AppendLine("</select></label>");
        sb.AppendLine("<button type=\"submit\">Spara assignment</button>");
        sb.AppendLine("</form>");
        sb.AppendLine("<table><thead><tr><th>MAC</th><th>Profil</th></tr></thead><tbody>");
        foreach (var assignment in assignments)
            sb.AppendLine($"<tr><td><code>{WebUtility.HtmlEncode(assignment.Mac)}</code></td><td>{WebUtility.HtmlEncode(assignment.ProfileName)}</td></tr>");
        sb.AppendLine("</tbody></table></section>");
        sb.AppendLine("</div>");

        sb.AppendLine("<div class=\"grid\">");
        sb.AppendLine("<section class=\"panel\"><h2>iPXE endpoints</h2><p><code>/api/boot?mac=${net0/mac}</code></p><p><code>/generated/menu.ipxe</code></p><form method=\"post\" action=\"/api/generate\"><button type=\"submit\">Generera statisk menu.ipxe</button></form></section>");
        sb.AppendLine("<section class=\"panel\"><h2>Filer</h2><p>Lägg bootfiler under <code>eutherboot/www/boot</code> och matcha paths i profilerna.</p><p class=\"muted\">ISO-extraktion är nästa separata steg.</p></section>");
        sb.AppendLine("</div>");

        sb.AppendLine("</main></body></html>");
        return sb.ToString();
    }
}
