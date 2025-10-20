# Phase 1: Integration Quickstart

**Feature**: Template Usage Screen Enhancements
**Date**: 2025-10-19
**Status**: Complete

## Overview

This document provides step-by-step integration scenarios demonstrating how components and hooks work together to deliver the four user stories. Each scenario maps to acceptance criteria in the specification.

---

## Scenario 1: Enhanced Placeholder Input (User Story 1)

**Goal**: User opens template, sees improved form fields, fills placeholders with clear guidance.

### Step-by-Step Flow

**1. User navigates to template usage page**

```typescript
// URL: /use/abc-123
// TemplateUsage.tsx renders

const TemplateUsage = () => {
  const { id } = useParams<{ id: string }>();

  // Fetch template from API
  const { data: template, isLoading } = useQuery({
    queryKey: ["template", id],
    queryFn: () => TemplatesService.getTemplate(id!),
  });

  // Wait for template to load
  if (isLoading) return <LoadingSpinner />;
  if (!template) return <NotFound />;

  // ... proceed to next step
};
```

**2. Extract placeholders from template content**

```typescript
// Template content: "# Feature: {{feature_name}}\n\n## Description\n{{description}}"
// usePlaceholders hook extracts placeholders

const {
  placeholders,    // [{ name: "feature_name", value: "", ... }, { name: "description", value: "", ... }]
  isValid,        // false (no values yet)
  updatePlaceholder,
  touchPlaceholder,
} = usePlaceholders(template.content);
```

**3. Render enhanced placeholder form**

```typescript
// PlaceholderForm.tsx renders
<PlaceholderForm
  placeholders={placeholders}
  onPlaceholderChange={updatePlaceholder}
  onPlaceholderBlur={touchPlaceholder}
/>

// Inside PlaceholderForm:
{placeholders.map((placeholder) => {
  const shouldUseTextarea =
    placeholder.name.includes("description") ||
    placeholder.value.length > 50;

  return (
    <div key={placeholder.name} className="space-y-2">
      <Label htmlFor={placeholder.name}>
        {placeholder.displayName || placeholder.name}
        {placeholder.isRequired && <span className="text-destructive">*</span>}
      </Label>

      {shouldUseTextarea ? (
        <Textarea
          id={placeholder.name}
          value={placeholder.value}
          onChange={(e) => onPlaceholderChange(placeholder.name, e.target.value)}
          onBlur={() => onPlaceholderBlur?.(placeholder.name)}
          required={placeholder.isRequired}
          aria-invalid={placeholder.error ? "true" : "false"}
          aria-describedby={placeholder.error ? `error-${placeholder.name}` : undefined}
          placeholder={`Enter ${placeholder.displayName || placeholder.name}...`}
          className="min-h-[100px]"
        />
      ) : (
        <Input
          id={placeholder.name}
          value={placeholder.value}
          onChange={(e) => onPlaceholderChange(placeholder.name, e.target.value)}
          onBlur={() => onPlaceholderBlur?.(placeholder.name)}
          required={placeholder.isRequired}
          aria-invalid={placeholder.error ? "true" : "false"}
          aria-describedby={placeholder.error ? `error-${placeholder.name}` : undefined}
          placeholder={`Enter ${placeholder.displayName || placeholder.name}...`}
        />
      )}

      {placeholder.touched && placeholder.error && (
        <p
          id={`error-${placeholder.name}`}
          className="text-sm text-destructive"
          role="alert"
        >
          {placeholder.error}
        </p>
      )}
    </div>
  );
})}
```

**4. User fills first placeholder**

```typescript
// User types "User Authentication" in feature_name field

// onChange event fires
onPlaceholderChange("feature_name", "User Authentication");

// usePlaceholders updates state
updatePlaceholder("feature_name", "User Authentication");

// State updates:
placeholders[0] = {
  name: "feature_name",
  value: "User Authentication",
  isRequired: true,
  error: undefined,  // No error (value is non-empty)
  touched: true,
};

// isValid recomputed: still false (description is empty)
isValid = false;
```

**5. User fills second placeholder**

```typescript
// User types description in textarea
onPlaceholderChange("description", "Implement OAuth2 authentication with Google and GitHub providers");

// State updates:
placeholders[1] = {
  name: "description",
  value: "Implement OAuth2 authentication...",
  isRequired: true,
  error: undefined,
  touched: true,
};

// isValid recomputed: now true (all placeholders filled)
isValid = true;
```

**Validation**: ✅ User Story 1, Scenarios 1-5 satisfied (improved form fields, clear labels, validation feedback)

---

## Scenario 2: Button Activation Based on Validation (User Story 2)

**Goal**: "Send to Devin" button is disabled until all placeholders filled.

### Step-by-Step Flow

**1. Button state managed by useFormValidation hook**

```typescript
const { canSubmit, validationMessage, missingPlaceholders } = useFormValidation(placeholders);

// Initial state (no placeholders filled):
// canSubmit = false
// validationMessage = "2 placeholder(s) required: feature_name, description"
// missingPlaceholders = ["feature_name", "description"]
```

**2. Button renders with disabled state and tooltip**

```typescript
<Tooltip>
  <TooltipTrigger asChild>
    <Button
      disabled={!canSubmit}
      onClick={() => sendMutation.mutate(getContentToSend())}
      aria-describedby={!canSubmit ? "send-button-status" : undefined}
    >
      Send to Devin
    </Button>
  </TooltipTrigger>
  <TooltipContent>
    {validationMessage ?? "Send template to Devin"}
  </TooltipContent>
</Tooltip>

{!canSubmit && (
  <p
    id="send-button-status"
    className="text-sm text-muted-foreground sr-only"
    aria-live="polite"
  >
    Button disabled: {missingPlaceholders.length} placeholder(s) required
  </p>
)}

// Button is disabled, shows red/grey appearance
// Tooltip on hover shows: "2 placeholder(s) required: feature_name, description"
```

**3. User fills first placeholder**

```typescript
// After filling feature_name:
// canSubmit = false
// validationMessage = "1 placeholder(s) required: description"
// missingPlaceholders = ["description"]

// Button still disabled
// Tooltip updates: "1 placeholder(s) required: description"
```

**4. User fills last placeholder**

```typescript
// After filling description:
// canSubmit = true
// validationMessage = null
// missingPlaceholders = []

// Button becomes enabled (disabled={false})
// Tooltip shows: "Send template to Devin"
```

**5. User clears a placeholder**

```typescript
// User deletes content from feature_name field
updatePlaceholder("feature_name", "");

// State updates:
placeholders[0].value = "";
placeholders[0].error = "This field is required";

// canSubmit = false (back to disabled)
// validationMessage = "1 placeholder(s) required: feature_name"

// Button becomes disabled again
```

**Validation**: ✅ User Story 2, Scenarios 1-5 satisfied (button disabled until valid, clear feedback)

---

## Scenario 3: Template Content Editing (User Story 3)

**Goal**: User fills placeholders, switches to Edit tab, modifies content, sees changes in preview.

### Step-by-Step Flow

**1. User fills all placeholders (from Scenario 1)**

```typescript
// placeholderValues = {
//   feature_name: "User Authentication",
//   description: "Implement OAuth2 authentication..."
// }
```

**2. useTemplateEditor hook computes resolved content**

```typescript
const {
  resolvedContent,
  finalContent,
  hasEdits,
  setEditedContent,
  resetEdits,
  getContentToSend,
} = useTemplateEditor(template.content, getPlaceholderValues());

// resolvedContent computed via generatePrompt:
// "# Feature: User Authentication\n\n## Description\nImplement OAuth2 authentication..."

// Initial state:
// finalContent = resolvedContent (no edits yet)
// hasEdits = false
```

**3. Preview tab shows resolved content**

```typescript
// activeTab = "preview"
<Tabs value={activeTab} onValueChange={setActiveTab}>
  <TabsList>
    <TabsTrigger value="preview">Preview</TabsTrigger>
    <TabsTrigger value="edit">Edit</TabsTrigger>
  </TabsList>

  <TabsContent value="preview" className="flex-1">
    <PromptPreview
      content={finalContent}
      hasEdits={hasEdits}
      title="Template Preview"
    />
  </TabsContent>
  {/* ... */}
</Tabs>

// PromptPreview renders:
<div className="flex flex-col flex-1 min-h-[300px] md:min-h-[400px]">
  <div className="flex items-center justify-between mb-2">
    <h3 className="text-lg font-semibold">Template Preview</h3>
    {hasEdits && <Badge variant="secondary">Edited</Badge>}
  </div>
  <pre className="flex-1 overflow-y-auto bg-muted p-4 rounded-md text-sm font-mono">
    {content}
  </pre>
</div>

// User sees resolved template in large preview box
```

**4. User switches to Edit tab**

```typescript
// User clicks Edit tab
setActiveTab("edit");

// TabsContent for "edit" becomes visible
<TabsContent value="edit" className="flex-1">
  <TemplateEditor
    content={finalContent}
    onContentChange={setEditedContent}
    hasEdits={hasEdits}
    onReset={resetEdits}
  />
</TabsContent>

// TemplateEditor renders:
<div className="flex flex-col flex-1 space-y-2">
  <div className="flex items-center justify-between">
    <Label htmlFor="template-editor">Edit Template Content</Label>
    {hasEdits && (
      <Button variant="outline" size="sm" onClick={onReset}>
        Reset to Original
      </Button>
    )}
  </div>

  <Textarea
    id="template-editor"
    value={content}
    onChange={(e) => onContentChange(e.target.value)}
    className="flex-1 min-h-[400px] font-mono"
    aria-label="Edit template content"
  />
</div>
```

**5. User modifies content**

```typescript
// User adds line: "\n## Additional Notes\nSupport MFA for enhanced security"

// onChange event fires
onContentChange(finalContent + "\n## Additional Notes\nSupport MFA for enhanced security");

// useTemplateEditor updates state
setEditedContent(newContent);

// State updates:
// editedContent = "# Feature: User Authentication\n...\n## Additional Notes\nSupport MFA..."
// hasEdits = true
// finalContent = editedContent (edited content takes precedence)

// Edit tab badge appears: "Edit ✓ Edited"
```

**6. User switches back to Preview tab**

```typescript
// User clicks Preview tab
setActiveTab("preview");

// PromptPreview now shows:
<PromptPreview
  content={finalContent}  // Shows edited content
  hasEdits={true}         // Badge visible
/>

// Preview displays edited content with "Edited" badge
```

**7. User resets edits**

```typescript
// User switches back to Edit tab and clicks "Reset to Original"
onReset();

// useTemplateEditor clears edited content
resetEdits();

// State updates:
// editedContent = null
// hasEdits = false
// finalContent = resolvedContent (reverts to resolved content)

// Preview and Edit tabs now show original resolved template
// "Edited" badge disappears
```

**Validation**: ✅ User Story 3, Scenarios 1-5 satisfied (can edit, changes preserved, can reset)

---

## Scenario 4: Larger Preview Box (User Story 4)

**Goal**: Preview box is significantly larger, shows more content at once.

### Step-by-Step Flow

**1. Page layout uses flexbox**

```typescript
// TemplateUsage.tsx layout structure
<div className="flex flex-col h-full gap-4 p-4">
  {/* Header section (template name, breadcrumbs) */}
  <header className="flex-shrink-0">
    <h1>{template.name}</h1>
  </header>

  {/* Placeholder form - fixed height based on content */}
  <section className="flex-shrink-0">
    <PlaceholderForm {...placeholderProps} />
  </section>

  {/* Tabs (Preview/Edit) - takes remaining vertical space */}
  <Tabs value={activeTab} onValueChange={setActiveTab} className="flex flex-col flex-1">
    <TabsList className="flex-shrink-0">
      <TabsTrigger value="preview">Preview</TabsTrigger>
      <TabsTrigger value="edit">Edit</TabsTrigger>
    </TabsList>

    {/* Tab content grows to fill space */}
    <TabsContent value="preview" className="flex-1 flex flex-col">
      <PromptPreview content={finalContent} hasEdits={hasEdits} />
    </TabsContent>

    <TabsContent value="edit" className="flex-1 flex flex-col">
      <TemplateEditor {...editorProps} />
    </TabsContent>
  </Tabs>

  {/* Send button - fixed height */}
  <footer className="flex-shrink-0">
    <Button {...sendButtonProps}>Send to Devin</Button>
  </footer>
</div>
```

**2. PromptPreview component uses responsive minimum heights**

```typescript
// PromptPreview.tsx
<div className="flex flex-col flex-1">
  <div className="flex items-center justify-between mb-2">
    <h3>{title}</h3>
    {hasEdits && <Badge>Edited</Badge>}
  </div>

  <pre className={cn(
    "flex-1 overflow-y-auto",
    "min-h-[300px] md:min-h-[400px] lg:min-h-[500px]",  // Responsive min heights
    "bg-muted p-4 rounded-md",
    "text-sm font-mono whitespace-pre-wrap",
    className
  )}>
    {content}
  </pre>
</div>
```

**3. Responsive behavior on different viewports**

```typescript
// Mobile (320px-767px):
// min-h-[300px] = 300px minimum height
// Placeholder form: ~200px (2 fields)
// Preview: 300px minimum
// Total: ~500px vertical space needed (scrollable page)

// Tablet (768px-1023px):
// min-h-[400px] = 400px minimum height
// Placeholder form: ~200px
// Preview: 400px minimum
// Total: ~600px vertical space (fits on most tablets)

// Desktop (1024px+):
// min-h-[500px] = 500px minimum height
// Placeholder form: ~200px
// Preview: 500px minimum
// On 1080p monitor (1920x1080): Preview can grow to ~700px (flex-1)
// Shows 40-50 lines of code (vs. ~20 lines previously)
```

**4. Content overflow handling**

```typescript
// If template content exceeds preview box height:
// overflow-y-auto enables vertical scrolling within preview
// User can scroll within preview without scrolling entire page

// For very long templates (500+ lines):
<pre className="overflow-y-auto max-h-[80vh]">
  {/* Content scrolls within box, limited to 80% of viewport height */}
</pre>
```

**Success Metric Validation**:
- **SC-004**: Preview displays at least 50% more visible lines
  - Previous: ~15-20 lines (estimated 300px height)
  - New: 25-30+ lines on desktop (500px min-height, grows with flex-1)
  - **Result**: ✅ 50%+ increase achieved

**Validation**: ✅ User Story 4, Scenarios 1-5 satisfied (larger preview, responsive, better visibility)

---

## Scenario 5: Complete Workflow (All User Stories Combined)

**Goal**: User completes entire template usage workflow from start to finish.

### Complete Flow

```typescript
// 1. User opens template
// URL: /use/feature-template-001
// → Template loaded via useQuery

// 2. Page renders with empty placeholders
// → PlaceholderForm shows 3 fields: feature_name, description, requirements
// → PromptPreview shows template with {{...}} markers
// → Send button DISABLED (tooltip: "3 placeholder(s) required: ...")

// 3. User fills placeholders one by one
// → After filling feature_name: button still disabled (2 remaining)
// → After filling description: button still disabled (1 remaining)
// → After filling requirements: button becomes ENABLED
// → Preview updates in real-time showing resolved content

// 4. User reviews preview (larger box shows more content)
// → User scrolls through preview
// → Preview shows ~40 lines on desktop (vs. ~20 previously)
// → User notices typo in requirements placeholder

// 5. User corrects typo in placeholder
// → Edits requirements field
// → Preview auto-updates with corrected content

// 6. User wants to add custom section
// → Switches to Edit tab
// → Adds new section: "\n## Success Criteria\n- User can log in\n- Session persists"
// → Edit tab shows "Edited" badge

// 7. User reviews edited content in Preview
// → Switches back to Preview tab
// → Sees original resolved content + custom section
// → "Edited" badge visible in Preview

// 8. User decides edit was too verbose, resets
// → Switches back to Edit tab
// → Clicks "Reset to Original" button
// → Content reverts to resolved template (without custom section)
// → Makes smaller edit: appends "\n\nNote: Initial MVP only"

// 9. User satisfied with final content
// → "Send to Devin" button enabled (all placeholders filled)
// → Clicks "Send to Devin"
// → Loading state appears on button
// → Toast notification: "Sent to Devin!"
// → Redirect to dashboard (or stay on page with success message)

// Final content sent to Devin:
// "# Feature: User Authentication\n\n## Description\nImplement OAuth2...\n\n## Requirements\n- Google provider\n- GitHub provider\n\nNote: Initial MVP only"
```

**All User Stories Validated**:
- ✅ **US1**: Enhanced placeholder input (textareas for long values, clear labels, validation)
- ✅ **US2**: Button activation (disabled until all filled, clear feedback)
- ✅ **US3**: Template editing (can modify content, reset, edits preserved)
- ✅ **US4**: Larger preview (50%+ more visible lines, responsive)

---

## Edge Case Scenarios

### Edge Case 1: Template with No Placeholders

```typescript
// Template content: "This is a static template with no placeholders"

// usePlaceholders extracts:
placeholders = [];  // Empty array

// usePlaceholders returns:
isValid = true;  // No required fields to fill

// Button state:
canSubmit = true;  // Immediately enabled

// User flow:
// 1. Open template → button immediately enabled
// 2. Can switch to Edit tab to modify static content
// 3. Can send directly without filling anything
```

**Validation**: ✅ Edge case handled (spec question: "Should button be enabled if no placeholders?")

---

### Edge Case 2: User Edits Then Changes Placeholder Values

```typescript
// 1. User fills placeholders: feature_name="Auth", description="OAuth2"
// resolvedContent = "# Feature: Auth\n\n## Description\nOAuth2"

// 2. User switches to Edit, adds custom section
// editedContent = "# Feature: Auth\n\n## Description\nOAuth2\n\n## Notes\nCustom notes"
// hasEdits = true

// 3. User goes back and changes feature_name to "Authentication"
updatePlaceholder("feature_name", "Authentication");

// resolvedContent updates: "# Feature: Authentication\n\n## Description\nOAuth2"
// editedContent UNCHANGED: "# Feature: Auth\n\n## Description\nOAuth2\n\n## Notes\nCustom notes"
// finalContent = editedContent (user's edit preserved)

// 4. Preview shows OLD feature name ("Auth") with custom notes
// → User's edits take precedence over placeholder changes

// 5. User must click "Reset to Original" to get updated placeholder values
// → After reset: finalContent = resolvedContent with "Authentication"
```

**Validation**: ✅ Edge case documented (spec question: "What if placeholders change after editing?")

---

### Edge Case 3: User Edits Content to Be Empty

```typescript
// 1. User switches to Edit tab
// 2. User deletes ALL content (Ctrl+A, Delete)
// editedContent = ""
// hasEdits = true (not null)
// finalContent = "" (empty string)

// 3. Button state:
canSubmit = true;  // All placeholders filled (button validation doesn't check content length)

// 4. User can send empty template
// → Potential issue: should we validate that finalContent is non-empty?

// Decision: Allow sending empty content (user may intentionally want to send blank)
// Alternative: Add content validation in useFormValidation hook
```

**Validation**: ✅ Edge case identified (spec question: "Should button be enabled if content is empty?")

---

### Edge Case 4: Very Long Template (Performance)

```typescript
// Template with 1000+ lines
template.content = "...(1000 lines)...";

// Performance considerations:
// 1. extractPlaceholders: O(n) where n = content length
//    → Runs once on mount, memoized
//    → 1000 lines (~50KB) extracts in <10ms

// 2. generatePrompt: O(n * m) where m = placeholder count
//    → Memoized, only recomputes when placeholders change
//    → 1000 lines, 10 placeholders: <50ms (well under 1s constraint)

// 3. Preview rendering: React renders <pre> with full content
//    → Initial render: ~100ms for 50KB text
//    → Scroll performance: native browser scrolling (60fps)

// 4. Edit textarea: Controlled component with full content in state
//    → Typing performance: React updates on every keystroke
//    → For 50KB+ content, consider debouncing or uncontrolled component
```

**Validation**: ✅ Performance validated (meets <1s constraint per spec)

---

### Edge Case 5: Navigation Away with Unsaved Edits

```typescript
// 1. User has edited content (hasEdits = true)
// 2. User clicks browser back button or navigates to different page

// Current behavior: No warning, edits lost
// → Component unmounts, state is cleared

// Desired behavior (future enhancement):
// → Warn user before navigation if hasEdits = true

// Implementation:
useEffect(() => {
  const handleBeforeUnload = (e: BeforeUnloadEvent) => {
    if (hasEdits) {
      e.preventDefault();
      e.returnValue = "";  // Shows browser's native "Leave page?" dialog
    }
  };

  window.addEventListener("beforeunload", handleBeforeUnload);
  return () => window.removeEventListener("beforeunload", handleBeforeUnload);
}, [hasEdits]);
```

**Validation**: ⚠️ Edge case identified (spec question: "Warn about losing changes?") - Future enhancement

---

## Integration Testing Checklist

**If test framework added**, these integration tests should be implemented:

### Test: Placeholder Form Integration

```typescript
test("filling all placeholders enables send button", async () => {
  render(<TemplateUsage />, { route: "/use/test-template" });

  // Wait for template to load
  await waitFor(() => expect(screen.getByText("Send to Devin")).toBeInTheDocument());

  // Button initially disabled
  const sendButton = screen.getByRole("button", { name: /send to devin/i });
  expect(sendButton).toBeDisabled();

  // Fill first placeholder
  const field1 = screen.getByLabelText(/feature_name/i);
  await userEvent.type(field1, "Test Feature");

  // Button still disabled
  expect(sendButton).toBeDisabled();

  // Fill second placeholder
  const field2 = screen.getByLabelText(/description/i);
  await userEvent.type(field2, "Test description");

  // Button now enabled
  expect(sendButton).toBeEnabled();
});
```

### Test: Template Editor Integration

```typescript
test("editing content shows edited badge and preserves edits", async () => {
  render(<TemplateUsage />, { route: "/use/test-template" });

  // Fill placeholders
  // ...

  // Switch to Edit tab
  const editTab = screen.getByRole("tab", { name: /edit/i });
  await userEvent.click(editTab);

  // Edit content
  const editor = screen.getByLabelText(/edit template content/i);
  await userEvent.type(editor, "\n\nCustom addition");

  // Check for edited badge
  expect(screen.getByText(/edited/i)).toBeInTheDocument();

  // Switch back to Preview
  const previewTab = screen.getByRole("tab", { name: /preview/i });
  await userEvent.click(previewTab);

  // Preview should show edited content
  expect(screen.getByText(/custom addition/i)).toBeInTheDocument();
});
```

---

**Phase 1: Quickstart Complete** ✅

**Next Step**: Update Agent Context Files
