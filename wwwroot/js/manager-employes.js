(() => {
  const TBODY = document.getElementById("emp-body");
  const INP   = document.getElementById("emp-search");
  const BTN   = document.getElementById("emp-search-btn");
  const CLR   = document.getElementById("emp-clear-btn");

  function badge(status, half) {
    const map = {
      available: "bg-success",
      sick: "bg-danger",
      vacation: "bg-warning",
      trip: "bg-primary",
      meeting: "bg-info",
      halfday: "bg-secondary",
      personal: "bg-dark"
    };
    const cls = map[status] || "bg-light text-dark";
    const text = status === "halfday" && half ? `half day (${half})` : status;
    return `<span class="badge ${cls} text-uppercase">${text}</span>`;
  }

  function fmtRange(from, to) {
    if (!from || !to) return "";
    const f = new Date(from), t = new Date(to);
    const ff = f.toLocaleDateString();
    const tt = t.toLocaleDateString();
    return ff === tt ? ff : `${ff} → ${tt}`;
  }

  function row(e) {
    return `
      <tr>
        <td>
          ${e.status === "available"
            ? ""
            : `<span class="d-inline-block rounded-circle" style="width:10px;height:10px;background:#${{
                sick:"dc3545", vacation:"ffc107", trip:"0d6efd", meeting:"0dcaf0", halfday:"6c757d", personal:"212529"
              }[e.status] || "adb5bd"}"></span>`
          }
        </td>
        <td>${e.name}</td>
        <td class="text-muted">${e.email}</td>
        <td>${badge(e.status, e.half)}</td>
        <td>${fmtRange(e.from, e.to)}</td>
        <td>
          <a class="btn btn-sm btn-outline-primary"
             href="/ManagerOnly/Calendar?user=${encodeURIComponent(e.email)}">Open calendar</a>
        </td>
      </tr>`;
  }

  async function load(q) {
    TBODY.innerHTML = `<tr><td colspan="6" class="text-center py-4">Loading…</td></tr>`;
    const url = q ? `/api/manager/employees/status?q=${encodeURIComponent(q)}` : `/api/manager/employes/status`;
    const r = await fetch(url, { credentials: "same-origin" });
    const data = r.ok ? await r.json() : [];
    if (!data.length) {
      TBODY.innerHTML = `<tr><td colspan="6" class="text-center py-4">No results</td></tr>`;
      return;
    }
    TBODY.innerHTML = data.map(row).join("");
  }

  BTN?.addEventListener("click", () => load(INP.value.trim()));
  CLR?.addEventListener("click", () => { INP.value = ""; load(""); });
  INP?.addEventListener("keydown", (e) => { if (e.key === "Enter") BTN.click(); });

  // Carga inicial
  load("");

  // --------- Live updates via SignalR ---------
  function waitForSignalR(maxMs = 8000, stepMs = 150) {
    return new Promise((res, rej) => {
      const t0 = Date.now();
      (function tick(){
        if (window.signalR && window.signalR.HubConnectionBuilder) return res();
        if (Date.now() - t0 > maxMs) return rej(new Error("SignalR timeout"));
        setTimeout(tick, stepMs);
      })();
    });
  }

  waitForSignalR().then(() => {
    const conn = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/notifications")
      .withAutomaticReconnect()
      .build();

    conn.on("unavailabilityCreated", () => {
      // recargar lista cuando alguien reporta
      load(INP.value.trim());
    });

    conn.start().catch(() => {});
  });

  // Auto refresco tras medianoche para limpiar estados caducados
  setInterval(() => {
    const now = new Date();
    if (now.getMinutes() === 0 && now.getSeconds() < 5) {
      load(INP.value.trim());
    }
  }, 5000);
  // ejemplo de manejador
    document.getElementById("calendar-search-go")?.addEventListener("click", async () => {
    const q = document.getElementById("calendar-search-input")?.value?.trim();
    if (!q) return;
    const r = await fetch(`/api/manager/resolve-user?q=${encodeURIComponent(q)}`, { credentials: "same-origin" });
    if (r.ok) {
        const { email } = await r.json();
        const url = new URL(location.href);
        url.searchParams.set("user", email);
        location.href = url.toString(); // navega al mismo calendario filtrado
    } else {
        alert("User not found");
    }
    });

})();
