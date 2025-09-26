# Applicant Management Web Solution - UI/UX Design Specifications

## Applicant Dashboard/List View

### Desktop Layout (1024px+)
**Header Section:**
- Top navigation bar: 64px height with white background
- Left side: Application logo and title "Applicant Management"
- Right side: "Create New Applicant" primary button (blue background, white text)
- Search bar: Full-width below header, 48px height with subtle border

**Data Table Structure:**
- Table header: Light gray background (#F8FAFC) with column titles
- Columns (left to right): 
  - Checkbox (for bulk actions)
  - Name (20% width)
  - Family Name (20% width)
  - Email Address (25% width)
  - Age (10% width, center aligned)
  - Hired Status (10% width, center aligned)
  - Actions (15% width, right aligned)
- Row height: 56px with alternating white/light-gray backgrounds
- Hover state: Subtle blue background (#EBF4FF) on row hover

**Action Buttons (per row):**
- View Details: Blue icon button with eye icon
- Edit: Green icon button with pencil icon
- Delete: Red icon button with trash icon
- Icons only on desktop, with tooltips on hover

**Pagination Footer:**
- Left: "Showing X-Y of Z applicants" text
- Center: Page numbers (1, 2, 3...)
- Right: Items per page dropdown (10, 25, 50, 100)

### Tablet Layout (768px - 1023px)
**Simplified Table:**
- Reduced columns: Name, Email, Hired Status, Actions
- Family Name and Age moved to expandable row details
- Action buttons remain but with larger touch targets (44px minimum)

### Mobile Layout (320px - 767px)
**Card-Based Layout:**
- Each applicant displayed as individual card
- Card height: Auto-adjust based on content
- Card content (top to bottom):
  - Header: Name and Family Name (bold, larger text)
  - Email: Secondary text color
  - Details row: Age | Country | Hired status badge
  - Action buttons: Full-width buttons stacked vertically
    - "View Details" (primary blue)
    - "Edit" (secondary outline)
    - "Delete" (danger red outline)

**Mobile Navigation:**
- Floating action button (+) for "Create New Applicant"
- Pull-to-refresh gesture support
- Swipe actions for quick actions (swipe left to reveal delete)

## Applicant Form (Create/Edit)

### Desktop Layout (Modal)
**Modal Specifications:**
- Width: 600px maximum
- Height: Auto-adjust with max 90vh
- Backdrop: Semi-transparent black (#000000, 50% opacity)
- Animation: Slide up from bottom (300ms ease-out)

**Form Structure:**
- Header: "Create New Applicant" or "Edit Applicant" title
- Close button: Top-right corner (X icon)
- Form fields (vertical layout, 24px spacing):

**Field Layout:**
```
[Label: Name *]
[Input Field: _________________________]
[Error: Please enter a valid name]

[Label: Family Name *]
[Input Field: _________________________]
[Error: Family name is required]

[Label: Email Address *]
[Input Field: _________________________]
[Error: Please enter a valid email]

[Two-column layout]
[Age *] [Country of Origin *]
[__] [_________________________]

[Label: Address]
[Textarea: _________________________]
[_________________________]

[Label: Hired Status]
[Toggle Switch: [ON] Hired / [OFF] Not Hired]
```

**Validation States:**
- **Default**: Gray border (#E2E8F0)
- **Focus**: Blue border (#2563EB) with blue glow
- **Valid**: Green border (#10B981) with green checkmark icon
- **Error**: Red border (#EF4444) with red error message below field
- **Disabled**: Light gray background (#F1F5F9) with gray text

**Button Actions:**
- Primary: "Save Applicant" (blue background)
- Secondary: "Cancel" (gray outline)
- Loading state: Button shows spinner, disabled during submission

### Mobile Layout (Full Screen)
**Full-Screen Form:**
- Takes entire viewport
- Sticky header with title and close button
- Scrollable form content
- Sticky footer with action buttons

**Mobile-Specific Features:**
- Larger touch targets (minimum 44px height)
- Native date picker for age field
- Dropdown picker for country selection
- Keyboard navigation optimized

## Applicant Details View

### Desktop Layout
**Page Structure:**
- Header section: 200px height with gradient background
- Back button: Top-left corner
- Edit/Delete buttons: Top-right corner
- Applicant name as page title (large, bold)

**Information Cards (3-column grid):**
**Left Column:**
- Profile Card:
  - Large avatar placeholder (120px circle)
  - Name (large, primary text)
  - Email (secondary text)
  - Hired status badge (green/red)

**Center Column:**
- Personal Information Card:
  - Family Name
  - Age
  - Country of Origin
  - Address (if provided)

**Right Column:**
- Actions Card:
  - Large "Edit Applicant" button (full width)
  - Large "Delete Applicant" button (outline style)
  - "Send Email" secondary action
  - Application history timeline

### Tablet Layout
**2-Column Grid:**
- Left: Profile and Personal Info combined
- Right: Actions and timeline
- Reduced spacing and font sizes

### Mobile Layout
**Single Column Stack:**
- Top: Back button + Action buttons (horizontal)
- Profile section with avatar and basic info
- Expandable sections for detailed information
- Sticky bottom action bar with primary actions

**Mobile Navigation:**
- Swipe left/right to navigate between applicants
- Pull-down to refresh data
- Share button for sending applicant details

## User Flow Description

### Primary User Journey
1. **Dashboard Entry**: User lands on applicant dashboard
2. **Browse/Search**: User reviews applicant list, uses search/filter
3. **Quick Actions**: User can edit/delete directly from list
4. **Detailed View**: Click on applicant for full details
5. **Edit Flow**: From details, click edit to modify information
6. **Create Flow**: Click "Create New" button to add applicant
7. **Validation**: Form validation prevents errors
8. **Confirmation**: Success message after save/delete actions

### Alternative Flows
- **Mobile Quick Actions**: Swipe for delete, tap for details
- **Bulk Operations**: Select multiple applicants for batch actions
- **Search Integration**: Real-time search filters results
- **Keyboard Navigation**: Full keyboard support for accessibility

### Error Handling
- **Network Errors**: Offline indicator with retry button
- **Validation Errors**: Inline field validation with clear messages
- **Delete Confirmation**: Modal confirmation before deletion
- **Form Errors**: Scroll to first error field on submission

### Accessibility Features
- **Screen Reader Support**: ARIA labels and semantic HTML
- **Keyboard Navigation**: Tab order and focus management
- **High Contrast**: WCAG 2.1 AA compliant color ratios
- **Touch Targets**: Minimum 44px for mobile interactions
- **Loading States**: Clear indicators during async operations