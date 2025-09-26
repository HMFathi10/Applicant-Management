# Applicant Management Web Solution - Visual Design Mockups

## Applicant Dashboard - Desktop View

```
┌─────────────────────────────────────────────────────────────────────────────────────────────────┐
│ Applicant Management                                    [+ Create New Applicant] 🔍 Search...  │
├─────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                 │
│ ┌─────────────────────────────────────────────────────────────────────────────────────────────┐ │
│ │                                                                                             │ │
│ │ Name          Family Name     Email Address              Age    Hired    Actions             │ │
│ │ ───────────── ───────────── ────────────────────────── ───── ──────── ───────────────────── │ │
│ │                                                                                             │ │
│ │ John          Smith           john.smith@email.com       28    ✓ Hired  [👁] [✏️] [🗑️]      │ │
│ │ Sarah         Johnson         sarah.j@company.com      32    ✗ Not    [👁] [✏️] [🗑️]      │ │
│ │ Michael       Chen            mchen@techcorp.com       45    ✓ Hired  [👁] [✏️] [🗑️]      │ │
│ │ Emily         Davis           emily.davis@email.com    29    ✗ Not    [👁] [✏️] [🗑️]      │ │
│ │                                                                                             │ │
│ │                                                                                             │ │
│ │                                                                                             │ │
│ │                                                                                             │ │
│ └─────────────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                                 │
│ Showing 1-10 of 124 applicants                    [1] 2 3 4 5 ...  [10 ▼ per page]         │
└─────────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Visual Elements:**
- Clean white background with subtle card shadows
- Alternating row colors for better readability
- Status badges with green (hired) and red (not hired) colors
- Icon-only action buttons with hover tooltips
- Professional blue header with high-contrast text

## Applicant Dashboard - Mobile View

```
┌─────────────────────┐
│ 🔍 Search...    [+] │
├─────────────────────┤
│ ┌─────────────────┐ │
│ │ John Smith      │ │
│ │ john.smith@...  │ │
│ │ Age: 28 | USA   │ │
│ │ [HIRED]         │ │
│ │ [View] [Edit]   │ │
│ │ [Delete]        │ │
│ └─────────────────┘ │
│ ┌─────────────────┐ │
│ │ Sarah Johnson   │ │
│ │ sarah.j@com...  │ │
│ │ Age: 32 | UK    │ │
│ │ [NOT HIRED]     │ │
│ │ [View] [Edit]   │ │
│ │ [Delete]        │ │
│ └─────────────────┘ │
│ ┌─────────────────┐ │
│ │ Michael Chen    │ │
│ │ mchen@tech...   │ │
│ │ Age: 45 | China │ │
│ │ [HIRED]         │ │
│ │ [View] [Edit]   │ │
│ │ [Delete]        │ │
│ └─────────────────┘ │
└─────────────────────┘
```

**Mobile-Specific Features:**
- Card-based layout for easy touch interaction
- Stacked action buttons for better thumb reach
- Compact information display with essential details
- Floating action button for creating new applicants

## Applicant Form - Desktop Modal

```
┌─────────────────────────────────────────────────────────────┐
│                                                     [X]    │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ Create New Applicant                                    │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │                                                           │ │
│ │ Name *                                                    │ │
│ │ ┌─────────────────────────────────────────────────────┐   │ │
│ │ │ John                                                 │   │ │
│ │ └─────────────────────────────────────────────────────┘   │ │
│ │                                                           │ │
│ │ Family Name *                                             │ │
│ │ ┌─────────────────────────────────────────────────────┐   │ │
│ │ │ Smith                                                │   │ │
│ │ └─────────────────────────────────────────────────────┘   │ │
│ │                                                           │ │
│ │ Email Address *                                           │ │
│ │ ┌─────────────────────────────────────────────────────┐   │ │
│ │ │ john.smith@email.com                                 │   │ │
│ │ └─────────────────────────────────────────────────────┘   │ │
│ │                                                           │ │
│ │ Age *                    Country of Origin *               │ │
│ │ ┌──────────────┐        ┌────────────────────────┐      │ │
│ │ │ 28            │        │ United States          │      │ │
│ │ └──────────────┘        └────────────────────────┘      │ │
│ │                                                           │ │
│ │ Address (Optional)                                        │ │
│ │ ┌─────────────────────────────────────────────────────┐   │ │
│ │ │ 123 Main Street, Apt 4B                             │   │ │
│ │ │ New York, NY 10001                                  │   │ │
│ │ └─────────────────────────────────────────────────────┘   │ │
│ │                                                           │ │
│ │ Hired Status                                              │ │
│ │ [Toggle Switch: ON] Hired                                 │ │
│ │                                                           │ │
│ │ ┌──────────────────┐      ┌──────────────────┐          │ │
│ │ │  Save Applicant  │      │     Cancel       │          │ │
│ │ └──────────────────┘      └──────────────────┘          │ │
│ │                                                           │ │
└─┴───────────────────────────────────────────────────────────┴─┘
```

**Form Validation States:**
```
Error State Example:
┌─────────────────────┐
│ Email Address *     │
│ ┌─────────────────┐ │
│ │ invalid-email   │ │  ← Red border
│ └─────────────────┘ │
│ ⚠️ Please enter a valid email address │  ← Red error text
└─────────────────────┘

Success State Example:
┌─────────────────────┐
│ Name *              │
│ ┌─────────────────┐ │
│ │ John Smith ✓    │ │  ← Green border with checkmark
│ └─────────────────┘ │
└─────────────────────┘
```

## Applicant Details View - Desktop

```
┌──────────────────────────────────────────────────────────────────────────────────────────────┐
│ [← Back]                                    [Edit Applicant] [Delete Applicant]             │
├──────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                              │
│ ┌─────────────┐ ┌─────────────────────────────────┐ ┌──────────────────────────────────┐   │
│ │             │ │ Personal Information            │ │ Actions                         │   │
│ │   [Avatar]  │ ├─────────────────────────────────┤ ├──────────────────────────────────┤   │
│ │             │ │ Name: John Smith               │ │ [📝 Edit Applicant]            │   │
│ │             │ │ Family Name: Smith             │ │ [🗑️ Delete Applicant]          │   │
│ │ John Smith  │ │ Age: 28 years old              │ │ [📧 Send Email]                │   │
│ │             │ │ Country: United States         │ │                                 │   │
│ │             │ │ Address:                       │ │ Application Timeline:          │   │
│ │ john.smith@ │ │ 123 Main Street                │ │ • Applied: March 15, 2024     │   │
│ │ email.com   │ │ New York, NY 10001             │ │ • Reviewed: March 18, 2024    │   │
│ │             │ │                                │ │ • Hired: March 25, 2024       │   │
│ │ [HIRED]     │ │ Contact:                       │ │                                 │   │
│ │             │ │ john.smith@email.com           │ │                                 │   │
│ └─────────────┘ └─────────────────────────────────┘ └──────────────────────────────────┘   │
│                                                                                              │
└──────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Design Elements:**
- Gradient header background (blue to light blue)
- Card-based layout with subtle shadows
- Status badge with prominent coloring
- Timeline visualization for application process
- Clear information hierarchy with proper spacing

## Applicant Details View - Mobile

```
┌─────────────────────┐
│ [←] John Smith  [⚙️]│
├─────────────────────┤
│ ┌─────────────────┐ │
│ │   [Avatar]      │ │
│ │   John Smith    │ │
│ │   [HIRED]       │ │
│ └─────────────────┘ │
│ ┌─────────────────┐ │
│ │ Personal Info   │ │
│ │ ─────────────── │ │
│ │ Name: John      │ │
│ │ Family: Smith   │ │
│ │ Age: 28         │ │
│ │ Country: USA    │ │
│ │ Email: john@... │ │
│ └─────────────────┘ │
│ ┌─────────────────┐ │
│ │ Address         │ │
│ │ ─────────────── │ │
│ │ 123 Main Street │ │
│ │ New York, NY    │ │
│ │ 10001           │ │
│ └─────────────────┘ │
│ ┌─────────────────┐ │
│ │ Timeline        │ │
│ │ ─────────────── │ │
│ │ Applied: Mar 15 │ │
│ │ Reviewed: Mar 18│ │
│ │ Hired: Mar 25   │ │
│ └─────────────────┘ │
│ ┌─────────────────┐ │
│ │ [Edit] [Delete] │ │
│ │ [Send Email]    │ │
│ └─────────────────┘ │
└─────────────────────┘
```

**Mobile Interaction Features:**
- Collapsible sections to save screen space
- Swipe gestures for navigation between applicants
- Sticky action buttons at bottom for easy thumb access
- Optimized typography for mobile readability

## Validation Error Handling Visual Examples

### Field Validation States
```
Default State:
┌─────────────────────┐
│ Name *              │
│ ┌─────────────────┐ │
│ │                   │ │
│ └─────────────────┘ │
└─────────────────────┘

Focus State:
┌─────────────────────┐
│ Name *              │
│ ┌─────────────────┐ │
│ │                   │ │ ← Blue border + blue glow
│ └─────────────────┘ │
└─────────────────────┘

Error State:
┌─────────────────────┐
│ Name *              │
│ ┌─────────────────┐ │
│ │ J                 │ │ ← Red border
│ └─────────────────┘ │
│ ⚠️ Name must be at least 2 characters │ ← Red error text
└─────────────────────┘

Success State:
┌─────────────────────┐
│ Name *              │
│ ┌─────────────────┐ │
│ │ John Smith ✓    │ │ ← Green border + checkmark
│ └─────────────────┘ │
└─────────────────────┘
```

### Form-Level Error Handling
```
Submit with Errors:
┌─────────────────────────────────────────────────────────────┐
│ ⚠️ Please correct the following errors:                     │
│ • Email address is invalid                                  │
│ • Age must be between 18 and 65                            │
│ • Country of origin is required                            │
└─────────────────────────────────────────────────────────────┘
```

**Error Prevention Features:**
- Real-time validation as user types
- Clear, actionable error messages
- Visual indicators (colors, icons) for quick recognition
- Auto-scroll to first error field on form submission
- Field-specific help text for complex validations