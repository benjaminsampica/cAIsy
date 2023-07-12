export function focusLastMessage() {
    const elements = document.getElementsByClassName("message");

    const lastElementIndex = elements.length - 1;
    const lastElement = elements.item(lastElementIndex);

    lastElement.scrollIntoView();
}