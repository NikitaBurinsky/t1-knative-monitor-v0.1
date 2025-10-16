const API_BASE = "http://localhost:8080/runner/echo";

// ====== GET запрос при загрузке ======
async function loadMetrics() {
    try {
        const response = await fetch(`${API_BASE}/get-metrics`);
        const data = await response.json();
        renderMetrics(data);
    } catch (err) {
        document.getElementById("metrics-content").innerText = "Ошибка загрузки данных: " + err;
    }
}

function calculateBilling(data) {
    const CPU_RATE = 0.00005;
    const RAM_RATE = 0.00001;
    const REQ_RATE = 0.001;

    const avgCpu = data.metrics.cpuStats.avgCpu ?? 0;
    const avgRam = data.metrics.ramStats?.avgRamStatsMB ?? 0;
    const requests = data.metrics.requestStats.requestCount ?? 0;
    const avgRunTime = data.metrics.mainRunningTimeStats.avgRunningTime ?? 0;

    const cpuCost = avgCpu * avgRunTime * CPU_RATE;
    const ramCost = avgRam * avgRunTime * RAM_RATE;
    const reqCost = requests * REQ_RATE;
    const total = cpuCost + ramCost + reqCost;

    document.getElementById("billing-content").innerHTML = `
    <p><b>Формулы:</b></p>
    <ul>
      <li>CPU: avgCpu × avgRunTime × ${CPU_RATE}</li>
      <li>RAM: avgRam(MB) × avgRunTime × ${RAM_RATE}</li>
      <li>Requests: requests × ${REQ_RATE}</li>
    </ul>
    <p><b>Подставленные значения:</b></p>
    <ul>
      <li>CPU: ${avgCpu} × ${avgRunTime} × ${CPU_RATE} = ${cpuCost.toFixed(6)} $</li>
      <li>RAM: ${avgRam} × ${avgRunTime} × ${RAM_RATE} = ${ramCost.toFixed(6)} $</li>
      <li>Requests: ${requests} × ${REQ_RATE} = ${reqCost.toFixed(6)} $</li>
    </ul>
    <p><b>Итого: ${total.toFixed(6)} $</b></p>
  `;
}

function renderMetrics(data) {
    const container = document.getElementById("metrics-content");

    container.innerHTML = `
    <table>
      <tr><th>ID</th><td>${data.id}</td></tr>
      <tr><th>Full Name</th><td>${data.fullname}</td></tr>
      <tr><th>Revision</th><td>${data.revisionName}</td></tr>
      <tr><th>Serving</th><td>${data.servingName}</td></tr>
      <tr><th>Pod</th><td>${data.podName}</td></tr>
      <tr><th>CPU (avg/max)</th><td>${data.metrics.cpuStats.avgCpu ?? "-"} / ${data.metrics.cpuStats.maxCpu ?? "-"}</td></tr>
      <tr><th>Requests</th><td>${data.metrics.requestStats.requestCount}</td></tr>
      <tr><th>Running Time (avg/max)</th><td>${data.metrics.mainRunningTimeStats.avgRunningTime ?? "-"} / ${data.metrics.mainRunningTimeStats.maxRunningTime ?? "-"}</td></tr>
      <tr><th>RAM (avg/max)</th><td>${data.metrics.ramStats?.avgRamStatsMB ?? "-"} / ${data.metrics.ramStats?.maxRamStatsMB ?? "-"}</td></tr>
    </table>
  `;

    calculateBilling(data);
}

// ====== POST запрос ======
document.getElementById("runner-form").addEventListener("submit", async (e) => {
    e.preventDefault();

    const runTimes = document.getElementById("runTimes").value;
    const runDelay = document.getElementById("runDelay").value;

    try {
        const response = await fetch(API_BASE, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ runTimes: parseInt(runTimes), runDelay: parseInt(runDelay) })
        });

        if (!response.ok) throw new Error("Ошибка: " + response.status);

        const result = await response.json();
        document.getElementById("runner-result").innerText = "Успешно выполнено: " + JSON.stringify(result);
    } catch (err) {
        document.getElementById("runner-result").innerText = "Ошибка выполнения: " + err;
    }
});

// ====== Переключатель темы ======
document.getElementById("theme-toggle").addEventListener("click", () => {
    document.body.classList.toggle("dark");
    const btn = document.getElementById("theme-toggle");
    if (document.body.classList.contains("dark")) {
        btn.textContent = "☀️ Светлый режим";
    } else {
        btn.textContent = "🌙 Тёмный режим";
    }
});

document.querySelector("h1").addEventListener("dblclick", () => {
    document.getElementById("easter-egg").style.display = "block";
});

let sessionStart = null;
let sessionEnd = null;
let sessionTimer = null;

function updateSessionInfo() {
    const infoDiv = document.getElementById("session-info");

    let durationText = "";
    if (sessionStart && !sessionEnd) {
        const now = new Date();
        const durationSec = Math.round((now - sessionStart) / 1000);
        durationText = `<p><b>Длительность (текущая):</b> ${durationSec} сек</p>`;
    } else if (sessionStart && sessionEnd) {
        const durationSec = Math.round((sessionEnd - sessionStart) / 1000);
        durationText = `<p><b>Длительность (итог):</b> ${durationSec} сек</p>`;
    }

    infoDiv.innerHTML = `
    <p><b>Начало сеанса:</b> ${sessionStart ? sessionStart.toLocaleString() : "—"}</p>
    <p><b>Конец сеанса:</b> ${sessionEnd ? sessionEnd.toLocaleString() : "—"}</p>
    ${durationText}
  `;
}

document.getElementById("start-period").addEventListener("click", () => {
    sessionStart = new Date();
    sessionEnd = null;

    if (sessionTimer) clearInterval(sessionTimer);
    sessionTimer = setInterval(updateSessionInfo, 1000);

    updateSessionInfo();
});

document.getElementById("end-period").addEventListener("click", () => {
    if (!sessionStart) {
        alert("Сначала начните сеанс!");
        return;
    }
    sessionEnd = new Date();
    if (sessionTimer) clearInterval(sessionTimer);
    updateSessionInfo();
});

document.getElementById("generate-invoice").addEventListener("click", () => {
    if (!sessionStart || !sessionEnd) {
        alert("Нужно завершить сеанс перед выставлением счёта!");
        return;
    }

    const durationMs = sessionEnd - sessionStart;
    const durationSec = Math.round(durationMs / 1000);

    const TIME_RATE = 0.00002;
    const timeCost = durationSec * TIME_RATE;

    const billingBlock = document.getElementById("billing-content");
    billingBlock.innerHTML += `
    <p><b>Сеанс:</b> ${durationSec} сек × ${TIME_RATE} $ = ${timeCost.toFixed(6)} $</p>
    <p><i>Временные рамки: ${sessionStart.toLocaleString()} — ${sessionEnd.toLocaleString()}</i></p>
  `;

    updateSessionInfo();
});

// ====== Автозагрузка ======
window.onload = loadMetrics;
