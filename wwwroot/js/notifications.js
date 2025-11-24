// wwwroot/js/notifications.js
(() => {

  // ---------- Esperar a que SignalR esté disponible ----------
  function waitForSignalR(maxMs, stepMs) {
    maxMs = maxMs || 8000;
    stepMs = stepMs || 150;

    return new Promise(function (resolve, reject) {
      var t0 = Date.now();
      (function tick() {
        if (window.signalR && window.signalR.HubConnectionBuilder) return resolve();
        if (Date.now() - t0 > maxMs) return reject(new Error("SignalR did not load in time"));
        setTimeout(tick, stepMs);
      })();
    });
  }

  // ---------- UI helpers ----------
  function renderBadge(n) {
    var el = document.getElementById("pending-badge");
    if (!el) return;
    el.textContent = n;
    if (n && n > 0) {
      el.classList.remove("d-none");
      var dot = document.querySelector(".hamburger .notif-dot");
      if (dot) dot.classList.add("show");
    } else {
      el.classList.add("d-none");
      var dot2 = document.querySelector(".hamburger .notif-dot");
      if (dot2) dot2.classList.remove("show");
    }
  }

  function showToast(text) {
    if (!window.bootstrap) {
      console.log("[toast]", text);
      return;
    }
    var box = document.getElementById("notif-toast");
    if (!box) {
      box = document.createElement("div");
      box.id = "notif-toast";
      box.className = "toast align-items-center position-fixed top-0 end-0 m-3";
      box.setAttribute("role", "alert");
      box.style.zIndex = "1080";
      box.innerHTML =
        '<div class="d-flex">' +
        '  <div class="toast-body"></div>' +
        '  <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>' +
        '</div>';
      document.body.appendChild(box);
    }
    box.querySelector(".toast-body").textContent = text;
    new bootstrap.Toast(box, { delay: 5000 }).show();
  }

  function showCard(html, href) {
    var id = "notif-card";
    var card = document.getElementById(id);
    if (!card) {
      card = document.createElement("div");
      card.id = id;
      card.className = "position-fixed top-0 end-0 m-3 p-3 shadow rounded bg-light";
      card.style.zIndex = "2000";
      card.style.minWidth = "340px";
      card.innerHTML =
        '<div class="d-flex align-items-start gap-2">' +
        '  <div class="flex-grow-1">' +
        '    <div class="fw-semibold"></div>' +
        '    <a class="btn btn-dark btn-sm mt-2" target="_self">Go to pending requests</a>' +
        '  </div>' +
        '  <button type="button" class="btn-close" aria-label="Close"></button>' +
        '</div>';
      document.body.appendChild(card);
      card.querySelector(".btn-close").onclick = function () { card.remove(); };
    }
    card.querySelector(".fw-semibold").innerHTML = html;
    var link = card.querySelector("a");
    if (href) {
      link.href = href;
      link.classList.remove("d-none");
    } else {
      link.href = "#";
      link.classList.add("d-none");
    }
    setTimeout(function () {
      if (card && card.parentNode) card.remove();
    }, 8000);
  }

  // ---------- Anti-duplicados ----------
  function onceSession(key) {
    try {
      var k = "once:" + key;
      if (sessionStorage.getItem(k)) return false;
      sessionStorage.setItem(k, "1");
      return true;
    } catch (e) {
      return true;
    }
  }

  // ---------- Arranque ----------
  waitForSignalR()
    .then(function () {
      console.log("[notif] SignalR listo, iniciando…");

      var connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notifications")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // ===== Eventos en tiempo real =====

      // 1) Manager: contador de pendientes
      connection.on("pendingCountUpdated", function (payload) {
        var count = payload && payload.count ? payload.count : 0;
        renderBadge(count);
      });

      // 2) Manager: nueva solicitud (vacation / sick / holiday / halfday)
      connection.on("vacationRequestCreated", function (p) {
        if (!p) return;
        if (!onceSession("vr:" + p.id)) return;

        showCard("Pending request of: " + (p.user || ""), "/ManagerOnly/Requests");

        var dot = document.querySelector(".hamburger .notif-dot");
        if (dot) dot.classList.add("show");
      });

      // 3) Usuario: decisión tomada (aprobada / no aprobada)
      connection.on("requestDecision", function (payload) {
        console.log("[notif] requestDecision recibido:", payload);

        if (!payload) return;
        var id = payload.id || payload.requestId;
        if (!id) return;
        if (!onceSession("decision:" + id)) return;

        var status = (payload.status || "").toString().toLowerCase();
        var ok = (status === "approved");
        var msg = "Your request #" + id + " was " + (ok ? "accepted" : "not accepted");

        showToast(msg);
      });

      // 4) Manager: unavailability directa (meeting / trip, etc.)
      connection.on("unavailabilityCreated", function (p) {
        if (!p) return;
        if (!onceSession("unav:" + p.id)) return;

        var pretty = p.prettyKind || "unavailability";
        var title = (p.user || "Someone") + " reported a " + pretty;
        var body = p.justification ? "<br/><small>" + p.justification + "</small>" : "";

        showCard(title + body, "/ManagerOnly/Calendar");
      });

      // ---------- Conectar ----------
      function start() {
        connection.start()
          .then(function () {
            console.log("[notif] conectado");
          })
          .catch(function (err) {
            console.warn("[notif] fallo de conexión, reintento en 3s", err);
            setTimeout(start, 3000);
          });
      }

      start();
    })
    .catch(function (err) {
      console.warn("[notif] SignalR no cargó:", err && (err.message || err));
    });

})();