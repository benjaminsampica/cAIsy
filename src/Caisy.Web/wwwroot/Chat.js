
function GetAllChatHistory() {
    var values = [],
    keys = Object.keys(localStorage),
    i = keys.length;

    while (i--) {
        values.push(localStorage.getItem(keys[i]));
    }

    var filteredResult = values.filter(item => item.includes("Source"));

    return filteredResult;
}