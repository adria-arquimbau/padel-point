var quill;

function initializeQuill(elementId) {
    var container = document.getElementById(elementId);
    if (container) {
        quill = new Quill(container, {
            theme: 'snow'
        });
        console.log("Quill initialized successfully.");
    } else {
        console.error("Element with ID", elementId, "not found.");
    }
}

function getQuillContent() {
    if (quill && quill.root) {
        return quill.root.innerHTML;
    } else {
        console.error("Quill not initialized or Quill root not found.");
        return "";
    }
}

function setQuillContent(content) {
    if (quill && quill.root) {
        quill.root.innerHTML = content;
    } else {
        console.error("Quill not initialized or Quill root not found.");
    }
}