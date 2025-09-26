# Applicant Management Web Solution - UI/UX Design System

## Project Overview
Applicant Management Web Solution with .NET Web API backend and React.js frontend, designed to streamline the recruitment process for HR professionals, recruiters, and hiring managers.

## Target Audience
HR professionals, recruiters, and hiring managers who need an efficient, intuitive system to manage job applicants throughout the recruitment pipeline.

## Design Philosophy
- **Professional & Modern**: Clean, corporate aesthetic that builds trust
- **Efficiency-First**: Minimize clicks, maximize information density
- **Accessible**: WCAG 2.1 AA compliant with high contrast ratios
- **Responsive**: Seamless experience across desktop, tablet, and mobile
- **Intuitive**: Self-explanatory interfaces that require minimal training

## Color Palette

### Primary Colors
- **Primary Blue**: #2563EB (Professional, trustworthy)
- **Primary Hover**: #1D4ED8 (Darker for interactions)
- **Primary Light**: #3B82F6 (Lighter variant)

### Neutral Colors
- **Background**: #FFFFFF (Clean white)
- **Surface**: #F8FAFC (Light gray background)
- **Border**: #E2E8F0 (Subtle borders)
- **Text Primary**: #1E293B (High contrast dark)
- **Text Secondary**: #64748B (Secondary information)

### Status Colors
- **Success**: #10B981 (Green for positive actions)
- **Warning**: #F59E0B (Amber for warnings)
- **Error**: #EF4444 (Red for errors)
- **Info**: #3B82F6 (Blue for information)

## Typography

### Font Family
- **Primary**: Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif
- **Monospace**: 'SF Mono', Monaco, 'Cascadia Code', monospace

### Font Sizes
- **Heading 1**: 2.25rem (36px) - Page titles
- **Heading 2**: 1.875rem (30px) - Section headers
- **Heading 3**: 1.5rem (24px) - Card titles
- **Body Large**: 1.125rem (18px) - Important content
- **Body Regular**: 1rem (16px) - Standard text
- **Body Small**: 0.875rem (14px) - Secondary text
- **Caption**: 0.75rem (12px) - Labels and captions

## Component Library

### Buttons
```css
/* Primary Button */
.btn-primary {
  background-color: #2563EB;
  color: white;
  padding: 12px 24px;
  border-radius: 8px;
  font-weight: 500;
  transition: all 0.2s ease;
}

.btn-primary:hover {
  background-color: #1D4ED8;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
}
```

### Form Elements
```css
/* Input Fields */
.form-input {
  border: 1px solid #E2E8F0;
  border-radius: 8px;
  padding: 12px 16px;
  font-size: 16px;
  transition: border-color 0.2s ease;
}

.form-input:focus {
  outline: none;
  border-color: #2563EB;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
}

.form-input.error {
  border-color: #EF4444;
}

.form-input.error:focus {
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}
```

### Cards & Containers
```css
.card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  padding: 24px;
  transition: box-shadow 0.2s ease;
}

.card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}
```

## Layout Grid System
- **Desktop**: 12-column grid with 24px gutters
- **Tablet**: 8-column grid with 16px gutters  
- **Mobile**: 4-column grid with 16px gutters
- **Max Container Width**: 1280px
- **Side Margins**: 24px (desktop), 16px (mobile)

## Responsive Breakpoints
- **Mobile**: 320px - 767px
- **Tablet**: 768px - 1023px
- **Desktop**: 1024px - 1279px
- **Large Desktop**: 1280px+