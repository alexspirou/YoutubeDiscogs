chrome.action.onClicked.addListener(() => {
     const width = 900;
     const height = 700;
     const left = Math.floor((screen.availWidth - width) / 2);
     const top = Math.floor((screen.availHeight - height) / 2);
   
     chrome.windows.create({
       url: "popup.html",
       type: "popup",
       width,
       height,
       left,
       top
     });
   });
   