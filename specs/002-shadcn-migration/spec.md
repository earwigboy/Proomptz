# Feature Specification: Migrate Frontend to shadcn/ui Components

**Feature Branch**: `002-shadcn-migration`
**Created**: 2025-10-19
**Status**: Draft
**Input**: User description: "Migrate existing frontend components to standard shadcn components"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Core Form Migration (Priority: P1)

Replace custom form inputs, buttons, and dialogs with standardized shadcn/ui components to improve consistency and maintainability across the template management interface.

**Why this priority**: Forms are the primary interaction point for users (creating/editing templates and folders). Standardizing these components first delivers immediate visual consistency and improved accessibility while establishing the foundation for future component migrations.

**Independent Test**: Can be fully tested by creating, editing, and deleting templates and folders through the UI. All form interactions (input fields, buttons, select dropdowns, dialogs) should render correctly and maintain existing functionality with improved visual consistency and accessibility.

**Acceptance Scenarios**:

1. **Given** a user wants to create a new template, **When** they click "Create New Template" and fill out the form, **Then** the form uses shadcn Input, Textarea, Select, and Button components with consistent styling
2. **Given** a user is editing an existing template, **When** the form loads, **Then** all form controls render with proper shadcn component styling and maintain current functionality
3. **Given** a user wants to create a folder, **When** they click the new folder button, **Then** a shadcn Dialog component appears with shadcn Input and Button components
4. **Given** a user submits a form with validation errors, **When** errors occur, **Then** error messages display using shadcn error styling patterns
5. **Given** a user is filling out a form, **When** they interact with disabled states during loading, **Then** shadcn loading states appear correctly on buttons and inputs

---

### User Story 2 - Card and List Migration (Priority: P2)

Replace custom template cards and list layouts with shadcn Card components to provide a more polished and consistent visual presentation of template data.

**Why this priority**: After forms work correctly, improving the visual presentation of template lists enhances the overall user experience. Cards are visible on every page load and represent the primary browsing interface.

**Independent Test**: Can be tested independently by navigating to the template list view and verifying that all templates render as shadcn Cards with proper layout, actions, and interactions. The feature delivers value by improving visual consistency even if other migrations aren't complete.

**Acceptance Scenarios**:

1. **Given** a user views the template list, **When** templates are loaded, **Then** each template displays as a shadcn Card component with proper header, content, and footer sections
2. **Given** a user views a template card, **When** hovering over action buttons, **Then** shadcn Button hover states appear correctly
3. **Given** a user views an empty template list, **When** no templates exist, **Then** an appropriate empty state displays using shadcn typography patterns
4. **Given** a user views template metadata, **When** looking at placeholder counts and dates, **Then** shadcn Badge and typography components display the information clearly

---

### User Story 3 - Navigation and Search Enhancement (Priority: P3)

Migrate folder tree navigation and search functionality to use shadcn navigation components (Collapsible, Tree patterns) and Input components with proper icons.

**Why this priority**: Navigation works well currently but can benefit from shadcn's accessibility features and visual polish. This is lower priority since it doesn't block core functionality and the existing implementation is functional.

**Independent Test**: Can be tested by expanding/collapsing folders, selecting folders, searching templates, and verifying keyboard navigation works properly. Delivers improved accessibility and visual consistency for navigation patterns.

**Acceptance Scenarios**:

1. **Given** a user views the folder tree, **When** folders are displayed, **Then** each folder uses shadcn Collapsible or Tree-style components with proper expand/collapse icons
2. **Given** a user searches for templates, **When** typing in the search box, **Then** the shadcn Input component displays with a proper search icon and clear button
3. **Given** a user navigates folders with keyboard, **When** using arrow keys and enter, **Then** keyboard navigation works correctly with proper focus indicators
4. **Given** a user drags a template to a folder, **When** hovering over folders, **Then** drop zones display proper hover states using shadcn styling patterns

---

### User Story 4 - Alerts and Feedback Migration (Priority: P4)

Replace custom alerts, loading states, and error messages with shadcn Alert, Skeleton, and Toast components for consistent user feedback.

**Why this priority**: Feedback components enhance UX but the application already has working alerts. This is the final polish layer that standardizes all feedback patterns but isn't essential for core functionality.

**Independent Test**: Can be tested by triggering various user actions (delete confirmations, loading states, success messages, error conditions) and verifying shadcn feedback components appear correctly. Independently valuable for creating consistent feedback patterns.

**Acceptance Scenarios**:

1. **Given** a user deletes a template, **When** confirming deletion, **Then** a shadcn AlertDialog component displays the confirmation prompt
2. **Given** templates are loading, **When** data is being fetched, **Then** shadcn Skeleton components display as loading placeholders
3. **Given** a user successfully saves a template, **When** the save completes, **Then** a shadcn Toast/Sonner notification appears with success feedback
4. **Given** an error occurs, **When** an API call fails, **Then** shadcn Alert component displays the error with appropriate styling and icon

---

### Edge Cases

- What happens when shadcn component props don't perfectly match existing custom component APIs?
- How does the system handle progressive enhancement if shadcn dependencies fail to load?
- What happens to existing CSS custom styles that conflict with shadcn default styles?
- How are drag-and-drop interactions maintained when migrating to new component structures?
- What happens to existing accessibility attributes (ARIA labels, roles) during migration - are they preserved or enhanced?
- How does the system handle component size variations when existing layouts expect specific dimensions?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST initialize shadcn/ui by running `npx shadcn@latest init` with appropriate configuration (TypeScript paths, style preferences, color theme)
- **FR-002**: System MUST replace all custom form input elements with shadcn Input components while maintaining existing validation logic
- **FR-003**: System MUST replace all custom button elements with shadcn Button components preserving variant styles (primary, secondary, destructive, ghost)
- **FR-004**: System MUST replace custom dialog overlays (FolderDialog) with shadcn Dialog components maintaining modal behavior and keyboard interactions
- **FR-005**: System MUST replace custom template cards with shadcn Card components maintaining existing layout structure (header, content, footer)
- **FR-006**: System MUST replace custom select dropdowns with shadcn Select components maintaining hierarchical folder display in template forms
- **FR-007**: System MUST replace custom textarea elements with shadcn Textarea components in template content editing
- **FR-008**: System MUST replace custom loading states with shadcn Skeleton or Spinner components
- **FR-009**: System MUST replace window.confirm() calls with shadcn AlertDialog components for deletion confirmations
- **FR-010**: System MUST replace custom error message displays with shadcn Alert components
- **FR-011**: System MUST implement shadcn Toast/Sonner for success notifications after create/update/delete operations
- **FR-012**: System MUST replace custom search input with shadcn Input component including search icon and clear button functionality
- **FR-013**: System MUST preserve all existing drag-and-drop functionality when migrating template cards and folder tree
- **FR-014**: System MUST maintain all existing accessibility attributes (ARIA labels, roles, keyboard navigation) or enhance them with shadcn's built-in accessibility
- **FR-015**: System MUST replace custom badge/count displays with shadcn Badge components for placeholder counts and folder template counts
- **FR-016**: System MUST configure shadcn theme to match or improve upon existing dark theme color scheme
- **FR-017**: System MUST maintain existing responsive behavior when migrating to shadcn components
- **FR-018**: System MUST replace folder tree expand/collapse icons with shadcn icon patterns (using lucide-react or similar)
- **FR-019**: System MUST preserve all existing form validation error messages and display them using shadcn error styling patterns
- **FR-020**: System MUST maintain existing loading state button text (e.g., "Saving...", "Loading...") when using shadcn Button loading states

### Key Entities

- **shadcn Component Configuration**: Defines the initialization settings including style framework (CSS variables vs. Tailwind classes), component path aliases, TypeScript configuration, icon library selection, and theme customization
- **Component Mapping**: Defines the relationship between existing custom components and their shadcn equivalents, including prop translations, styling migrations, and behavioral equivalences
- **Theme Configuration**: Defines color variables, typography scale, spacing system, and dark mode implementation that matches existing visual design or provides approved improvements

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All form interactions (create template, edit template, create folder, rename folder) complete successfully using shadcn components with no functional regressions
- **SC-002**: Visual consistency improves measurably - all buttons, inputs, and cards follow a single design system instead of mixed custom styles
- **SC-003**: Accessibility score improves or maintains current level - all keyboard navigation, screen reader support, and ARIA attributes work correctly with shadcn components
- **SC-004**: Development velocity for future UI changes improves - adding new form fields or dialogs requires using standardized shadcn components instead of writing custom CSS
- **SC-005**: Code reduction occurs - custom component files (FolderDialog.css, FolderTree.css, custom form styles in App.css) are removed or significantly reduced in favor of shadcn component usage
- **SC-006**: Loading states and user feedback appear consistently - all loading indicators, success messages, and error alerts use the same shadcn patterns throughout the application
- **SC-007**: Dark theme remains functional or improves - all migrated shadcn components render correctly in the existing dark theme without visual regressions

## Scope Boundaries *(mandatory)*

### In Scope

- Installing and configuring shadcn/ui with appropriate settings for the React + TypeScript + Vite application
- Migrating all existing custom components identified in the analysis (TemplateForm, TemplateList, FolderDialog, FolderTree, SearchBar, SearchResults, PlaceholderForm, PromptPreview)
- Replacing custom CSS with shadcn component styling
- Implementing shadcn Dialog, AlertDialog, Card, Button, Input, Textarea, Select, Badge, Alert, Toast/Sonner, Skeleton components
- Configuring dark theme support for all shadcn components
- Maintaining all existing functionality during migration (no feature additions)
- Preserving accessibility features and keyboard navigation patterns
- Installing required dependencies (Radix UI primitives, class-variance-authority, clsx/tailwind-merge)

### Out of Scope

- Adding new features beyond component migration (e.g., new form fields, new user interactions)
- Redesigning the overall layout or information architecture
- Migrating to a different frontend framework or routing library
- Adding unit or integration tests (unless specifically requested later)
- Changing the backend API contracts or data models
- Performance optimization beyond what shadcn components provide by default
- Migrating away from existing state management (TanStack Query) or routing (React Router)
- Creating custom shadcn component variants beyond what's needed to match existing functionality
- Adding animations or transitions beyond shadcn defaults (unless they already exist)
- Changing existing business logic, validation rules, or data processing

## Assumptions *(mandatory)*

1. The existing React 19 + TypeScript + Vite setup is compatible with shadcn/ui (current stable version)
2. The project uses or will use Tailwind CSS for styling (shadcn's default approach) or CSS variables as an alternative
3. The existing dark theme color scheme will be replicated in shadcn theme configuration
4. All existing component behavior should be preserved - this is a UI component migration, not a functional redesign
5. The development team is comfortable with the shadcn/ui component library and its conventions
6. The Radix UI primitives that shadcn uses are acceptable dependencies for the project
7. Existing custom CSS files will be progressively removed as components are migrated
8. TypeScript path aliases (e.g., `@/components/ui/*`) will be configured for shadcn component imports
9. The migration will happen incrementally - components can be migrated one at a time without breaking the application
10. lucide-react or a similar icon library will be used for icons in shadcn components

## Dependencies & Constraints *(optional)*

### Dependencies

- Existing React 19 application must remain functional during migration
- TanStack Query (React Query) integration must continue working with shadcn components
- React Router navigation must work correctly with shadcn Dialog and other overlay components
- Existing API client and data models remain unchanged
- Bundle size should not increase significantly (shadcn uses tree-shaking, so only imported components are bundled)

### Constraints

- Cannot break existing functionality during migration - must maintain backward compatibility at each step
- Must preserve accessibility features that already exist (ARIA attributes, keyboard navigation)
- Must maintain existing dark theme without requiring users to reconfigure settings
- Migration should be completable in phases - each user story represents a deployable increment
- Cannot introduce breaking changes to component props used by parent components without updating those parents
- Must work within existing build tooling (Vite) without requiring major build configuration changes
