# Quickstart Guide: Shadcn Blocks UI Enhancement

**Feature**: 003-shadcn-blocks-ui
**Date**: 2025-10-19
**Audience**: Developers implementing the UI enhancement
**Prerequisites**: Feature 002 (shadcn migration) completed, Node.js 18+, npm/yarn

## Overview

This guide provides step-by-step instructions for implementing the shadcn Sidebar block and applying the blue theme. The implementation is organized into three independent phases matching the user stories (P1: Sidebar, P2: Blue Theme, P3: Layout Enhancements).

## Phase 0: Prerequisites Check

Before starting, verify the environment:

```bash
cd frontend

# Verify shadcn is installed (from feature 002)
cat components.json

# Verify current theme
grep "dark mode" src/index.css

# Check existing components
ls src/components/ui/
```

**Expected**: components.json exists, shadcn components present (button, card, dialog, etc.)

---

## Phase 1: Implement Shadcn Sidebar (User Story 1 - P1)

### Step 1: Install Sidebar Component

```bash
cd frontend
npx shadcn@latest add sidebar
```

**What this does**:
- Adds `src/components/ui/sidebar.tsx` (~500 lines, fully typed)
- Installs dependencies: `@radix-ui/react-slot`, `class-variance-authority`, `lucide-react` (if not present)
- Updates `components.json` registry

**Verify installation**:
```bash
ls src/components/ui/sidebar.tsx
# Should show the file exists
```

### Step 2: Update App Layout Structure

**File**: `frontend/src/App.tsx`

**Before** (current structure):
```tsx
function App() {
  return (
    <BrowserRouter>
      <div className="app-container">
        <header>
          <h1>Prompt Template Manager</h1>
        </header>
        <div className="main-content">
          <aside className="sidebar">
            {/* FolderTree component */}
          </aside>
          <main className="content-area">
            {/* Page content */}
          </main>
        </div>
      </div>
    </BrowserRouter>
  );
}
```

**After** (with Sidebar):
```tsx
import { SidebarProvider, Sidebar, SidebarContent, SidebarHeader, SidebarInset } from '@/components/ui/sidebar';
import { SidebarTrigger } from '@/components/ui/sidebar';
import { Separator } from '@/components/ui/separator';

function AppSidebar({ children }: { children: React.ReactNode }) {
  return (
    <Sidebar>
      <SidebarHeader>
        <h2 className="px-2 text-lg font-semibold">Folders</h2>
      </SidebarHeader>
      <SidebarContent>
        {children}
      </SidebarContent>
    </Sidebar>
  );
}

function App() {
  return (
    <BrowserRouter>
      <SidebarProvider defaultOpen={true}>
        <AppSidebar>
          {/* FolderTree will go here */}
        </AppSidebar>

        <SidebarInset>
          <header className="sticky top-0 z-10 flex h-16 shrink-0 items-center gap-2 border-b bg-background px-4">
            <SidebarTrigger />
            <Separator orientation="vertical" className="mr-2 h-4" />
            <h1 className="text-xl font-semibold">Prompt Template Manager</h1>
          </header>

          <main className="flex flex-1 flex-col gap-4 p-4 md:gap-8 md:p-6">
            <Routes>
              <Route path="/" element={<HomePage />} />
              <Route path="/search" element={<Search />} />
              <Route path="/use/:id" element={<TemplateUsage />} />
            </Routes>
          </main>
        </SidebarInset>
      </SidebarProvider>
    </BrowserRouter>
  );
}
```

**Key Changes**:
1. Wrap entire app with `<SidebarProvider defaultOpen={true}>`
2. Create `<AppSidebar>` component wrapping `<Sidebar>`
3. Use `<SidebarInset>` for main content area
4. Add sticky header with `<SidebarTrigger>` for collapse/expand
5. Apply responsive padding: `p-4 md:p-6` to main content

### Step 3: Integrate FolderTree into Sidebar

**File**: `frontend/src/App.tsx` (HomePage component)

**Current structure**:
```tsx
function HomePage() {
  // ... existing state and handlers ...

  return (
    <div className="app-container">
      <aside className="sidebar">
        <FolderTree
          folders={folderTree?.rootFolders}
          selectedFolderId={selectedFolderId}
          onFolderSelect={handleFolderSelect}
          onFolderContextMenu={handleFolderContextMenu}
          onCreateSubfolder={handleCreateFolder}
          onTemplateDrop={handleTemplateDrop}
        />
      </aside>
      <main>
        {/* Template list */}
      </main>
    </div>
  );
}
```

**Updated structure**:
```tsx
function App() {
  return (
    <BrowserRouter>
      <SidebarProvider defaultOpen={true}>
        <Routes>
          <Route path="/" element={<HomePage />} />
          {/* ... other routes */}
        </Routes>
      </SidebarProvider>
    </BrowserRouter>
  );
}

function HomePage() {
  // ... existing state and handlers ...

  return (
    <>
      <AppSidebar>
        {folderTree && folderTree.rootFolders && (
          <FolderTree
            folders={folderTree.rootFolders}
            selectedFolderId={selectedFolderId}
            onFolderSelect={handleFolderSelect}
            onFolderContextMenu={handleFolderContextMenu}
            onCreateSubfolder={handleCreateFolder}
            onTemplateDrop={handleTemplateDrop}
          />
        )}
      </AppSidebar>

      <SidebarInset>
        <header className="sticky top-0 z-10 flex h-16 items-center gap-2 border-b bg-background px-4">
          <SidebarTrigger />
          <Separator orientation="vertical" className="mr-2 h-4" />
          <h1>Prompt Template Manager</h1>
        </header>

        <main className="flex-1 p-4 md:p-6">
          {showForm ? (
            <TemplateForm {...formProps} />
          ) : (
            <TemplateList {...listProps} />
          )}
        </main>
      </SidebarInset>

      {/* Dialogs and modals */}
      {showFolderDialog && <FolderDialog {...} />}
      {contextMenu && <FolderContextMenu {...} />}
    </>
  );
}
```

### Step 4: Remove Old Sidebar CSS

**File**: `frontend/src/App.css`

Remove or comment out:
```css
/* OLD - Remove this */
.sidebar {
  width: 280px;
  background: #1a1a1a;
  border-right: 1px solid #444;
  overflow-y: auto;
}

/* Sidebar styles now handled by shadcn Sidebar component */
```

### Step 5: Test Sidebar Functionality

**Manual Testing Checklist**:

1. **Sidebar Display**:
   - [ ] Sidebar shows on left side with folder tree
   - [ ] Sidebar header displays "Folders" title
   - [ ] Folder tree renders correctly inside sidebar

2. **Collapse/Expand**:
   - [ ] Click trigger button in header - sidebar collapses
   - [ ] Click again - sidebar expands
   - [ ] State persists on page reload (check `sidebar_state` cookie in dev tools)

3. **Mobile Responsive**:
   - [ ] Resize to < 768px - sidebar becomes sheet overlay
   - [ ] Click trigger - sidebar slides in from left
   - [ ] Click outside or backdrop - sidebar closes
   - [ ] Resize to ≥ 768px - sidebar becomes fixed again

4. **Folder Operations**:
   - [ ] Expand/collapse folders still works
   - [ ] Select folder - templates filter correctly
   - [ ] Create new folder - dialog opens
   - [ ] Rename folder - dialog opens
   - [ ] Delete folder - confirmation works
   - [ ] Drag-drop template to folder - moves correctly

5. **Keyboard Navigation**:
   - [ ] Tab to trigger button - focus visible
   - [ ] Enter/Space - toggles sidebar
   - [ ] Esc (when sidebar open) - closes sidebar

**If any tests fail**: Check browser console for errors, verify FolderTree props passed correctly, ensure SidebarProvider wraps the entire component tree.

---

## Phase 2: Apply Blue Theme (User Story 2 - P2)

### Step 1: Update CSS Variables for Blue Theme

**File**: `frontend/src/index.css`

**Find the `.dark` section** and replace color variables:

```css
@layer base {
  .dark {
    /* Background & Surface Colors */
    --background: 224 71% 4%;           /* Deep blue-black */
    --foreground: 213 31% 91%;          /* Light blue-white text */
    --card: 224 71% 4%;                 /* Card background */
    --card-foreground: 213 31% 91%;     /* Card text */
    --popover: 224 71% 4%;              /* Popover background */
    --popover-foreground: 215 20.2% 65.1%; /* Popover text */

    /* Primary Colors (Vibrant Blue) */
    --primary: 221.2 83.2% 53.3%;       /* Vibrant blue #1e5efa */
    --primary-foreground: 210 40% 98%;  /* Almost white */

    /* Secondary & Accent */
    --secondary: 210 40% 96.1%;         /* Light blue-gray */
    --secondary-foreground: 222.2 47.4% 11.2%; /* Dark blue */
    --accent: 210 40% 96.1%;            /* Light blue accent */
    --accent-foreground: 222.2 47.4% 11.2%; /* Dark blue */

    /* Interactive Elements */
    --muted: 223 47% 11%;               /* Muted background */
    --muted-foreground: 215.4 16.3% 56.9%; /* Muted text */

    /* Borders & Inputs */
    --border: 216 34% 17%;              /* Subtle blue border */
    --input: 216 34% 17%;               /* Input border */
    --ring: 215 20.2% 65.1%;            /* Focus ring */

    /* Semantic Colors */
    --destructive: 0 100% 50%;          /* Red for errors */
    --destructive-foreground: 210 40% 98%; /* White text */

    /* Border Radius (keep existing or adjust) */
    --radius: 0.5rem;                   /* 8px rounded corners */
  }
}

/* Keep dark theme as default */
html {
  @apply dark;
}
```

### Step 2: Update Tailwind Configuration

**File**: `frontend/tailwind.config.js`

**Verify color configuration** (should already be set from feature 002):

```javascript
module.exports = {
  darkMode: ["class"],
  content: [
    "./pages/**/*.{ts,tsx}",
    "./components/**/*.{ts,tsx}",
    "./app/**/*.{ts,tsx}",
    "./src/**/*.{ts,tsx}",
  ],
  theme: {
    container: {
      center: true,
      padding: "2rem",
      screens: {
        "2xl": "1400px",
      },
    },
    extend: {
      colors: {
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
        },
        secondary: {
          DEFAULT: "hsl(var(--secondary))",
          foreground: "hsl(var(--secondary-foreground))",
        },
        destructive: {
          DEFAULT: "hsl(var(--destructive))",
          foreground: "hsl(var(--destructive-foreground))",
        },
        muted: {
          DEFAULT: "hsl(var(--muted))",
          foreground: "hsl(var(--muted-foreground))",
        },
        accent: {
          DEFAULT: "hsl(var(--accent))",
          foreground: "hsl(var(--accent-foreground))",
        },
        popover: {
          DEFAULT: "hsl(var(--popover))",
          foreground: "hsl(var(--popover-foreground))",
        },
        card: {
          DEFAULT: "hsl(var(--card))",
          foreground: "hsl(var(--card-foreground))",
        },
      },
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
    },
  },
  plugins: [require("tailwindcss-animate")],
}
```

**No changes needed** if this config is already in place from feature 002. Just verify it exists.

### Step 3: Remove Old Header CSS

**File**: `frontend/src/App.css`

Remove or comment out old header styles:

```css
/* OLD - Remove or comment out */
/* header {
  padding: 1rem 2rem;
  border-bottom: 2px solid #646cff;
  background: #1a1a1a;
}

header h1 {
  margin: 0;
  color: #646cff;
} */

/* Header styles now handled by blue theme CSS variables and Tailwind classes */
```

### Step 4: Test Blue Theme Application

**Visual Testing Checklist**:

1. **Sidebar**:
   - [ ] Sidebar background uses dark blue-black (`--background`)
   - [ ] Folder text uses light blue-white (`--foreground`)
   - [ ] Selected folder uses blue accent (`--primary`)
   - [ ] Hover states show subtle blue tint

2. **Buttons**:
   - [ ] Primary buttons use vibrant blue (`--primary`)
   - [ ] Outline buttons show blue border
   - [ ] Destructive buttons still show red
   - [ ] Hover states darken appropriately

3. **Cards**:
   - [ ] Template cards have dark blue background
   - [ ] Card borders show subtle blue tint (`--border`)
   - [ ] Text maintains good contrast (light on dark)
   - [ ] Badges use blue secondary color

4. **Dialogs/Forms**:
   - [ ] Dialog background uses card color (dark blue)
   - [ ] Input borders show blue tint
   - [ ] Focus rings use blue (`--ring`)
   - [ ] Labels and text are readable

5. **Alerts/Toasts**:
   - [ ] Success toasts use green (existing)
   - [ ] Error toasts use red (`--destructive`)
   - [ ] Info/default toasts use blue tones
   - [ ] Toast text is readable

**Contrast Validation**:

Use browser dev tools or online checker (e.g., WebAIM Contrast Checker):

```
Foreground (#e3e8ef) on Background (#060913)
Expected: > 4.5:1 (WCAG AA for normal text)

Primary (#1e5efa) on Background (#060913)
Expected: > 3:1 (WCAG AA for large text/UI components)
```

**If contrast fails**: Adjust `--foreground` or `--primary` lightness value in `index.css`.

---

## Phase 3: Layout Enhancements (User Story 3 - P3)

### Step 1: Update Header Block Pattern

**File**: `frontend/src/App.tsx` (header in SidebarInset)

**Already completed in Phase 1**, but verify it matches block pattern:

```tsx
<header className="sticky top-0 z-10 flex h-16 shrink-0 items-center gap-2 border-b bg-background px-4">
  <SidebarTrigger />
  <Separator orientation="vertical" className="mr-2 h-4" />
  <h1 className="text-xl font-semibold">Prompt Template Manager</h1>
</header>
```

**Specifications**:
- ✅ Height: `h-16` (64px)
- ✅ Sticky: `sticky top-0 z-10`
- ✅ Flex: `flex items-center`
- ✅ Gap: `gap-2` (8px)
- ✅ Border: `border-b`
- ✅ Background: `bg-background` (prevents transparency)
- ✅ Padding: `px-4` (16px)

### Step 2: Update Content Area Padding

**File**: `frontend/src/App.tsx` (main in SidebarInset)

**Before**:
```tsx
<main className="content-area">
  {/* content */}
</main>
```

**After**:
```tsx
<main className="flex flex-1 flex-col gap-4 p-4 md:gap-8 md:p-6 lg:p-8">
  {/* content */}
</main>
```

**Specifications**:
- Progressive padding: `p-4` (mobile) → `md:p-6` (tablet) → `lg:p-8` (desktop)
- Progressive gap: `gap-4` (mobile) → `md:gap-8` (desktop)
- Flex layout: `flex flex-1 flex-col` for full-height stretching

### Step 3: Review Template List Grid

**File**: `frontend/src/components/TemplateList.tsx`

**Verify grid pattern** (should already be in place from feature 002):

```tsx
<div className="templates-grid grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
  {templates.map(template => (
    <Card key={template.id}>
      {/* card content */}
    </Card>
  ))}
</div>
```

**Specifications**:
- ✅ Mobile: `grid-cols-1` (single column)
- ✅ Tablet: `md:grid-cols-2` (two columns)
- ✅ Desktop: `lg:grid-cols-3` (three columns)
- ✅ Gap: `gap-4` (16px consistent spacing)

**No changes needed** if grid is already using this pattern.

### Step 4: Test Responsive Layout

**Responsive Testing Checklist**:

Test at these viewport widths:

1. **Mobile (375px)**:
   - [ ] Sidebar hidden by default (sheet overlay)
   - [ ] Header shows trigger button
   - [ ] Content padding: 16px (`p-4`)
   - [ ] Template list: 1 column
   - [ ] All content readable and clickable

2. **Tablet (768px)**:
   - [ ] Sidebar visible (fixed, can collapse)
   - [ ] Content padding: 24px (`md:p-6`)
   - [ ] Template list: 2 columns
   - [ ] Adequate spacing between elements

3. **Desktop (1024px)**:
   - [ ] Sidebar fully functional with collapse
   - [ ] Content padding: 32px (`lg:p-8`)
   - [ ] Template list: 3 columns
   - [ ] Comfortable viewing experience

4. **Wide Desktop (1920px)**:
   - [ ] Layout doesn't stretch excessively
   - [ ] Content max-width reasonable
   - [ ] Sidebar proportional to screen

**Testing Tools**:
- Chrome DevTools responsive mode (Cmd/Ctrl + Shift + M)
- Test on actual devices if available
- Firefox Responsive Design Mode

---

## Verification & Quality Checks

### Build Verification

```bash
cd frontend

# TypeScript compilation
npx tsc -b

# Build for production
npm run build

# Check bundle size
npm run check-size
```

**Expected Results**:
- ✅ Zero TypeScript errors
- ✅ Build succeeds
- ✅ Bundle size increase < 50KB (sidebar component ~15-20KB)
- ✅ No console warnings

### Accessibility Validation

**Keyboard Navigation**:
1. Tab through all interactive elements - focus visible
2. Enter/Space on trigger - sidebar toggles
3. Esc when sidebar open - sidebar closes
4. Arrow keys in folder tree - navigation works

**Screen Reader**:
1. VoiceOver (Mac): Cmd + F5
2. NVDA (Windows): Download from nvaccess.org
3. Verify:
   - Sidebar announces open/closed state
   - Folder count announced
   - Buttons have clear labels

**Contrast Check**:
Use browser extensions:
- Chrome: WAVE Evaluation Tool
- Firefox: axe DevTools

**Expected**: Zero critical accessibility issues, WCAG AA compliance maintained.

### Performance Validation

**Check page load time**:

```bash
# Open browser DevTools
# Network tab -> Disable cache -> Reload
# Check "Load" time in Network summary
```

**Expected**: Page load increase < 100ms compared to before implementation.

**Bundle Analysis** (optional):

```bash
npm run build -- --analyze
```

**Expected**: Sidebar component shows ~15KB, no unexpected large dependencies.

---

## Rollback Plan

If critical issues arise:

### Option 1: Revert Specific Commit

```bash
# Find the commit
git log --oneline

# Revert it
git revert <commit-hash>
```

### Option 2: Revert Entire Feature

```bash
# Revert all commits on branch
git revert <first-commit-hash>^..<last-commit-hash>

# Or abandon branch
git checkout 002-shadcn-migration
git branch -D 003-shadcn-blocks-ui
```

### Option 3: Quick Disable (Testing)

Temporarily disable sidebar to test if it's causing issues:

```tsx
// In App.tsx
function App() {
  return (
    <BrowserRouter>
      {/* Temporarily remove SidebarProvider wrapper */}
      <div className="app-container">
        {/* Old layout structure */}
      </div>
    </BrowserRouter>
  );
}
```

---

## Troubleshooting

### Issue: Sidebar doesn't collapse/expand

**Symptoms**: Click trigger button, nothing happens

**Fixes**:
1. Check `SidebarProvider` wraps entire component tree
2. Verify `<SidebarTrigger />` is inside `<SidebarProvider>`
3. Check browser console for `useSidebar()` errors
4. Clear cookies and reload: `sidebar_state` cookie may be corrupted

### Issue: Blue theme not applying

**Symptoms**: Still see old purple/pink colors

**Fixes**:
1. Clear browser cache (Cmd/Ctrl + Shift + R)
2. Verify `index.css` has new CSS variables
3. Check `html { @apply dark; }` is present
4. Inspect element in dev tools - check computed `--primary` value
5. Rebuild: `npm run build`

### Issue: Mobile layout broken

**Symptoms**: Sidebar overlaps content on small screens

**Fixes**:
1. Verify `SidebarInset` wraps main content
2. Check mobile trigger has correct classes: `<SidebarTrigger className="md:hidden" />`
3. Test at exactly 768px breakpoint
4. Clear `sidebar_state` cookie

### Issue: Folder functionality broken

**Symptoms**: Can't create/rename/delete folders

**Fixes**:
1. Verify all props passed to `<FolderTree>`
2. Check folder handlers still defined in parent component
3. Ensure `<FolderDialog>` and `<FolderContextMenu>` render outside `<SidebarInset>`
4. Test without sidebar to isolate issue

### Issue: Contrast too low

**Symptoms**: Text hard to read on blue backgrounds

**Fixes**:
1. Increase lightness of `--foreground`: Change `213 31% 91%` to `213 31% 95%`
2. Decrease saturation of `--background`: Change `224 71% 4%` to `224 50% 4%`
3. Run WCAG checker: https://webaim.org/resources/contrastchecker/
4. Adjust until ratio > 4.5:1

---

## Next Steps

After completing all phases:

1. **Manual QA**: Test all user stories end-to-end
2. **Code Review**: Create PR for review
3. **Documentation**: Update README if needed
4. **Staging Deploy**: Deploy to staging environment
5. **User Acceptance**: Get feedback from users
6. **Production Deploy**: Merge to main and deploy

## Support Resources

- Shadcn Sidebar Docs: https://ui.shadcn.com/docs/components/sidebar
- Shadcn Blocks Examples: https://ui.shadcn.com/blocks
- Tailwind Spacing: https://tailwindcss.com/docs/customizing-spacing
- WCAG Contrast Checker: https://webaim.org/resources/contrastchecker/
- Project Constitution: `.specify/memory/constitution.md`
- Claude Context: `.specify/memory/claude/context.md`
