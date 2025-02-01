document.addEventListener("DOMContentLoaded", function () {
    const questionTypeDropdown = document.getElementById("Type");
    const numberOfItemsInput = document.getElementById("numberOfItems");
    const updateButton = document.getElementById("updateButton");
    const dynamicFields = document.getElementById("dynamicFields");
    const form = document.getElementById("questionForm");

    function renderFields() {
        const selectedType = questionTypeDropdown.value;
        const numberOfItems = parseInt(numberOfItemsInput.value) || 1;

        dynamicFields.innerHTML = ""; // Clear existing fields

        if (selectedType === "MultipleChoice" || selectedType === "MultipleResponse") {
            dynamicFields.innerHTML = "<h4>Options</h4>";
            for (let i = 0; i < numberOfItems; i++) {
                const isChecked = document.querySelector(`input[name="options[${i}].IsCorrect"]`)?.checked ?? false;
                const textValue = document.querySelector(`input[name="options[${i}].Text"]`)?.value ?? "";

                dynamicFields.innerHTML += `
                    <div class="form-group">
                        <label>Option ${i + 1}</label>
                        <input type="${selectedType === "MultipleChoice" ? "radio" : "checkbox"}" 
                               name="options[${i}].IsCorrect" value="true" ${isChecked ? "checked" : ""} />
                        <input type="text" name="options[${i}].Text" class="form-control" 
                               placeholder="Enter option ${i + 1}" value="${textValue}" required />
                    </div>`;
            }
        } else if (selectedType === "Matching") {
            dynamicFields.innerHTML = "<h4>Matching Pairs</h4>";
            for (let i = 0; i < numberOfItems; i++) {
                const leftValue = document.querySelector(`input[name="matchingPairs[${i}].LeftSideText"]`)?.value ?? "";
                const rightValue = document.querySelector(`input[name="matchingPairs[${i}].RightSideText"]`)?.value ?? "";

                dynamicFields.innerHTML += `
                    <div class="form-group">
                        <label>Left Side ${i + 1}</label>
                        <input type="text" name="matchingPairs[${i}].LeftSideText" 
                               class="form-control" placeholder="Enter left side ${i + 1}" 
                               value="${leftValue}" required />
                        <label>Right Side ${i + 1}</label>
                        <input type="text" name="matchingPairs[${i}].RightSideText" 
                               class="form-control" placeholder="Enter right side ${i + 1}" 
                               value="${rightValue}" required />
                    </div>`;
            }
        }
    }

    // Preserve previous values before updating
    function preserveState() {
        const inputs = dynamicFields.querySelectorAll("input");
        const storedValues = {};
        inputs.forEach(input => {
            storedValues[input.name] = input.value;
        });

        return storedValues;
    }

    function restoreState(storedValues) {
        Object.keys(storedValues).forEach(name => {
            const input = document.querySelector(`input[name="${name}"]`);
            if (input) {
                input.value = storedValues[name];
            }
        });
    }

    // Update fields when type or number changes
    questionTypeDropdown.addEventListener("change", function () {
        const state = preserveState();
        renderFields();
        restoreState(state);
    });

    updateButton.addEventListener("click", function () {
        const state = preserveState();
        renderFields();
        restoreState(state);
    });

    // Initial render
    renderFields();

    // Form submit validation
    form.addEventListener("submit", function (event) {
        const selectedType = questionTypeDropdown.value;
        if (selectedType !== "Matching" && dynamicFields.querySelectorAll("input[type='text']").length === 0) {
            event.preventDefault();
            alert("Please enter at least one option.");
        }
    });
});
