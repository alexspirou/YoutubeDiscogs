const currentUser = "John";
document.addEventListener("DOMContentLoaded", () => {
  document.getElementById("username").innerText = `Welcome, ${currentUser}`;
});

let releases = [];
const pageSize = 5;
let currentPage = 0;

function renderPage() {
  const start = currentPage * pageSize;
  const end = start + pageSize;
  const items = releases.slice(start, end);

  document.getElementById("grid").innerHTML = items.map(r => `
    <div class="release-card">
      <a href="https://www.discogs.com${r.uri}" target="_blank">
        <img src="${r.cover_image || r.thumb || 'https://via.placeholder.com/150'}" alt="${r.title}" />
      </a>
      <a class="release-title" href="https://www.discogs.com${r.resource_url}" target="_blank">${r.title}</a>
      <div class="release-artist">${r.country || "Unknown Country"} (${r.year || "n/a"})</div>
      <button class="wantlist-button" onclick="addToWantlist('${r.title}')">Add to Wantlist</button>
    </div>
  `).join('');

  document.getElementById("prevBtn").disabled = currentPage === 0;
  document.getElementById("nextBtn").disabled = end >= releases.length;
}

function setupPagination() {
  const controls = document.createElement("div");
  controls.id = "pagination-controls";
  controls.innerHTML = `
    <button id="prevBtn" class="pagination-button">Previous</button>
    <button id="nextBtn" class="pagination-button">Next</button>
  `;
  document.body.appendChild(controls);

  document.getElementById("prevBtn").addEventListener("click", () => {
    if (currentPage > 0) {
      currentPage--;
      renderPage();
    }
  });

  document.getElementById("nextBtn").addEventListener("click", () => {
    if ((currentPage + 1) * pageSize < releases.length) {
      currentPage++;
      renderPage();
    }
  });
}

function addToWantlist(title) {
  alert(`✅ '${title}' added to your wantlist (mock)`);
}

async function fetchReleases(query) {
  try {
    const res = await fetch(`https://localhost:7043/api/discogs/search?query=${encodeURIComponent(query)}`);
    const data = await res.json();
    releases = data.results || [];
    currentPage = 0;
    renderPage();
  } catch (err) {
    console.error("❌ Failed to fetch releases", err);
    document.getElementById("grid").innerHTML = "<p>❌ Failed to load releases.</p>";
  }
}

chrome.tabs.query({ active: true, currentWindow: true }, function (tabs) {
  const tab = tabs[0];
  const url = tab.url || "";
  const urlContainer = document.getElementById("youtube-url");

  if (url.includes("youtube.com/watch?v=")) {
    urlContainer.innerHTML = `
      <div><strong>Video:</strong> <a href="${url}" target="_blank">${url}</a></div>
      <div id="video-title" class="video-title">Loading title...</div>
      <div id="title-edit-container" style="margin-top: 6px;"></div>
    `;

    chrome.tabs.sendMessage(tab.id, { action: "getTitle" }, function (response) {
      const titleDiv = document.getElementById("video-title");
      const editContainer = document.getElementById("title-edit-container");

      if (chrome.runtime.lastError) {
        titleDiv.innerHTML = "<em>(Could not read title — try reloading YouTube tab)</em>";
        return;
      }

      if (response?.title) {
        const cleanTitle = response.title.replace(/^\(\d+\)\s*/, "");
        titleDiv.innerHTML = `<strong>Title:</strong> ${cleanTitle}`;
        editContainer.innerHTML = `
          <input type="text" id="editable-title" value="${cleanTitle}" style="width: 70%;" />
          <button id="resend-button">Resend</button>
        `;

        document.getElementById("resend-button").addEventListener("click", () => {
          const editedTitle = document.getElementById("editable-title").value;
          titleDiv.innerHTML = `<strong>Title:</strong> ${editedTitle}`;
          fetchReleases(editedTitle);
        });

        fetchReleases(cleanTitle);
      } else {
        titleDiv.innerHTML = "<em>(No title returned)</em>";
      }
    });
  } else {
    urlContainer.innerHTML = "⚠️ Not a YouTube video page.";
  }
});

setupPagination();
