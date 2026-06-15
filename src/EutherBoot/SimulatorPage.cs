using System.Net;
using System.Text;

namespace EutherBoot;

public static class SimulatorPage
{
    public static string Render(IReadOnlyList<BootProfile> profiles, string bootUrl)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html lang=\"sv\">");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\">");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
        sb.AppendLine("<title>EutherBoot Simulator</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{margin:0;font:15px/1.45 system-ui,sans-serif;background:#f4f6f8;color:#172026}main{max-width:1160px;margin:0 auto;padding:26px 18px 40px}.top{display:flex;align-items:flex-end;justify-content:space-between;gap:16px;margin-bottom:18px}h1{font-size:30px;line-height:1.1;margin:0}h2{font-size:17px;margin:0 0 12px}.muted{color:#62707b}.grid{display:grid;grid-template-columns:360px 1fr;gap:16px}.panel{background:white;border:1px solid #d9e0e7;border-radius:8px;padding:16px}.stack{display:grid;gap:12px}label{display:grid;gap:5px;font-weight:600}input,select,button{font:inherit}input,select{box-sizing:border-box;width:100%;border:1px solid #c8d1da;border-radius:6px;padding:9px;background:white}button{border:0;border-radius:6px;background:#1167a8;color:white;padding:10px 12px;cursor:pointer}button.secondary{background:#e8eef3;color:#172026}button.choice{width:100%;text-align:left;background:#f7f9fb;color:#172026;border:1px solid #d6dee6;padding:12px}.choice strong{display:block}.choice span{display:block;color:#61707c;font-size:13px;margin-top:2px}.screen{background:#101820;color:#d8f7d2;border-radius:8px;padding:16px;min-height:210px;font:14px/1.45 ui-monospace,SFMono-Regular,Consolas,monospace;white-space:pre-wrap}.bootbox{display:grid;gap:8px}.kv{display:grid;grid-template-columns:80px 1fr;gap:8px;border-top:1px solid #e7ebef;padding-top:8px}.kv code,code{background:#eef2f5;border-radius:4px;padding:2px 5px;overflow-wrap:anywhere}.actions{display:flex;gap:8px;flex-wrap:wrap}.ok{color:#1e7a3a}.warn{color:#9d5a00}@media(max-width:840px){.grid{grid-template-columns:1fr}.top{display:block}.top a{display:inline-block;margin-top:10px}}");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body><main>");
        sb.AppendLine("<div class=\"top\"><div><h1>EutherBoot iPXE-simulator</h1>");
        sb.AppendLine($"<div class=\"muted\">Anropar samma endpoint som riktig iPXE: <code>/api/boot?mac=...</code>. Boot-url: <code>{WebUtility.HtmlEncode(bootUrl)}</code></div></div>");
        sb.AppendLine("<a href=\"/\">Admin</a></div>");

        sb.AppendLine("<div class=\"grid\">");
        sb.AppendLine("<section class=\"panel stack\">");
        sb.AppendLine("<h2>Klient</h2>");
        sb.AppendLine("<label>MAC-adress<input id=\"mac\" value=\"52:54:00:12:34:56\"></label>");
        sb.AppendLine("<div class=\"actions\"><button id=\"load\">PXE-boota</button><button class=\"secondary\" id=\"clear\">Rensa assignment</button></div>");
        sb.AppendLine("<label>Sätt nästa boot<select id=\"profile\">");
        foreach (var profile in profiles)
            sb.AppendLine($"<option value=\"{WebUtility.HtmlEncode(profile.Name)}\">{WebUtility.HtmlEncode(profile.Label)}</option>");
        sb.AppendLine("</select></label>");
        sb.AppendLine("<button id=\"assign\">Spara MAC-assignment</button>");
        sb.AppendLine("<p class=\"muted\">Simulatorn laddar iPXE-scriptet, hittar menyval eller direkt bootblock och visar vad en PXE-klient skulle se.</p>");
        sb.AppendLine("</section>");

        sb.AppendLine("<section class=\"panel stack\">");
        sb.AppendLine("<h2>Simulerad skärm</h2>");
        sb.AppendLine("<div id=\"status\" class=\"muted\">Inte startad.</div>");
        sb.AppendLine("<div id=\"menu\" class=\"stack\"></div>");
        sb.AppendLine("<div id=\"boot\" class=\"bootbox\"></div>");
        sb.AppendLine("<h2>Rått iPXE-script</h2>");
        sb.AppendLine("<div id=\"screen\" class=\"screen\"></div>");
        sb.AppendLine("</section>");
        sb.AppendLine("</div>");

        sb.AppendLine("<script>");
        sb.AppendLine("""
const macInput = document.getElementById('mac');
const profileSelect = document.getElementById('profile');
const statusEl = document.getElementById('status');
const menuEl = document.getElementById('menu');
const bootEl = document.getElementById('boot');
const screenEl = document.getElementById('screen');

function parseScript(text) {
  const lines = text.split(/\r?\n/).map(line => line.trim()).filter(Boolean);
  const items = [];
  const blocks = {};
  let current = null;

  for (const line of lines) {
    if (line.startsWith('item ')) {
      const parts = line.split(/\s+/);
      items.push({ id: parts[1], label: parts.slice(2).join(' ') });
      continue;
    }
    if (line.startsWith(':')) {
      current = line.slice(1);
      blocks[current] = [];
      continue;
    }
    if (current) blocks[current].push(line);
  }

  return { items, blocks };
}

function describeBoot(id, block) {
  const kernel = block.find(line => line.startsWith('kernel ')) || '';
  const initrd = block.find(line => line.startsWith('initrd ')) || '';
  const args = kernel.split(/\s+/).slice(2).join(' ');
  bootEl.innerHTML = `
    <h2>Boot preview</h2>
    <div class="kv"><strong>Profil</strong><code>${escapeHtml(id)}</code></div>
    <div class="kv"><strong>Kernel</strong><code>${escapeHtml(kernel.replace(/^kernel\s+/, '').split(/\s+/)[0] || '')}</code></div>
    <div class="kv"><strong>Initrd</strong><code>${escapeHtml(initrd.replace(/^initrd\s+/, '') || '')}</code></div>
    <div class="kv"><strong>Args</strong><code>${escapeHtml(args || '(inga)')}</code></div>`;
}

function render(text) {
  const parsed = parseScript(text);
  screenEl.textContent = text;
  menuEl.innerHTML = '';
  bootEl.innerHTML = '';

  const bootIds = Object.keys(parsed.blocks).filter(id => id !== 'shell');
  if (parsed.items.length > 0) {
    statusEl.innerHTML = '<span class="ok">Meny mottagen.</span> Välj ett item för att se bootblocket.';
    for (const item of parsed.items) {
      const button = document.createElement('button');
      button.className = 'choice';
      button.innerHTML = `<strong>${escapeHtml(item.label)}</strong><span>${escapeHtml(item.id)}</span>`;
      button.onclick = () => {
        if (item.id === 'shell') {
          bootEl.innerHTML = '<h2>Boot preview</h2><p class="warn">Det här valet öppnar iPXE shell.</p>';
          return;
        }
        describeBoot(item.id, parsed.blocks[item.id] || []);
      };
      menuEl.appendChild(button);
    }
    return;
  }

  if (bootIds.length === 1) {
    statusEl.innerHTML = '<span class="ok">Direkt boot mottagen.</span> MAC-adressen har en assignment.';
    describeBoot(bootIds[0], parsed.blocks[bootIds[0]]);
    return;
  }

  statusEl.innerHTML = '<span class="warn">Kunde inte tolka scriptet.</span>';
}

async function boot() {
  const mac = encodeURIComponent(macInput.value.trim());
  const response = await fetch(`/api/boot?mac=${mac}`);
  render(await response.text());
}

async function assign() {
  const mac = encodeURIComponent(macInput.value.trim());
  const profile = encodeURIComponent(profileSelect.value);
  const response = await fetch(`/api/assignments?mac=${mac}&profile=${profile}`, { method: 'POST' });
  if (!response.ok) {
    statusEl.textContent = await response.text();
    return;
  }
  await boot();
}

async function clearAssignment() {
  const mac = encodeURIComponent(macInput.value.trim());
  await fetch(`/api/assignments?mac=${mac}`, { method: 'DELETE' });
  await boot();
}

function escapeHtml(value) {
  return value.replace(/[&<>"']/g, ch => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[ch]));
}

document.getElementById('load').onclick = boot;
document.getElementById('assign').onclick = assign;
document.getElementById('clear').onclick = clearAssignment;
boot();
""");
        sb.AppendLine("</script>");
        sb.AppendLine("</main></body></html>");

        return sb.ToString();
    }
}
