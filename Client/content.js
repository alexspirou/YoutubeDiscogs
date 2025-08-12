chrome.runtime.onMessage.addListener((msg, sender, sendResponse) => {
     if (msg.action === "getTitle") {
       const title = document.title.replace(" - YouTube", "").trim();
       sendResponse({ title });
     }
   });
   