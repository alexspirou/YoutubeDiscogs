window.chromeInterop = {
    getActiveTabUrlAndTitle: async () => {
        const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
        return new Promise((resolve) => {
            chrome.tabs.sendMessage(tab.id, { action: "getTitle" }, (res) => {
                resolve({ url: tab.url, title: res?.title });
            });
        });
    }
};
