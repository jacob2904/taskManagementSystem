# Task Management UI

A modern, responsive React application for managing tasks with a beautiful Material-UI interface.

## Features

- ✨ **Modern Design**: Built with Material-UI components and modern design principles
- 🌓 **Dark Mode**: Automatic theme detection with manual toggle support
- 📱 **Responsive**: Fully responsive design that works on mobile, tablet, and desktop
- 🎨 **Beautiful UI**: Cards, animations, and smooth transitions throughout
- 🔍 **Task Management**: Create, read, update, and delete tasks
- 🏷️ **Tag System**: Organize tasks with customizable tags
- 📊 **Dashboard**: Overview of tasks with statistics and recent tasks
- ⚡ **Fast**: Built with Vite for lightning-fast development and builds

## Tech Stack

- **React 19** - UI library
- **TypeScript** - Type safety
- **Material-UI** - Component library
- **React Router** - Navigation
- **Axios** - HTTP client
- **Date-fns** - Date formatting
- **Vite** - Build tool

## Prerequisites

- Node.js 16+ and npm
- .NET 9.0 SDK (for the backend API)
- The backend API should be running at `http://localhost:7071`

## Getting Started

### 1. Install Dependencies

```bash
npm install
```

### 2. Configure Environment

The application uses environment variables for configuration. The `.env` file is already configured:

```env
VITE_API_URL=http://localhost:7071/api
```

If your API runs on a different port, update this file accordingly.

### 3. Start the Backend API

Before running the frontend, make sure the backend API is running:

```bash
cd ../TaskManagement.API
dotnet run
```

The API should be accessible at `http://localhost:7071`

### 4. Start the Development Server

```bash
npm run dev
```

The application will open at `http://localhost:5173`

## Building for Production

```bash
npm run build
```

The built files will be in the `dist` directory.

## Project Structure

```
src/
├── components/          # Reusable UI components
│   ├── Header.tsx      # Top navigation bar
│   ├── Sidebar.tsx     # Side navigation menu
│   ├── TaskCard.tsx    # Task display card
│   └── TaskForm.tsx    # Task creation/edit form
├── pages/              # Main application pages
│   ├── Dashboard.tsx   # Dashboard with statistics
│   ├── Tasks.tsx       # Task list and management
│   └── Tags.tsx        # Tag management
├── services/           # API service layer
│   └── api.ts         # HTTP client and API calls
├── types/             # TypeScript type definitions
│   └── index.ts       # Task, Tag, User interfaces
├── hooks/             # Custom React hooks
│   └── useTheme.ts    # Theme management hook
├── theme/             # Material-UI theme configuration
│   └── theme.ts       # Light/dark theme setup
├── App.tsx            # Main app component with routing
└── main.tsx           # Application entry point
```

## Features Overview

### Dashboard

- View total tasks count
- See tasks by priority (High, Medium, Low)
- View recent tasks at a glance

### Tasks Page

- Grid view of all tasks with beautiful cards
- Create new tasks with detailed information
- Edit existing tasks
- Delete tasks with confirmation dialog
- Assign tags to tasks
- Set priority levels
- Assign tasks to users with contact details

### Tags Page

- Create and manage tags
- Edit tag names
- Delete unused tags
- View all tags in a list

## Theme System

The application supports both light and dark modes:

- Automatically detects system theme preference
- Manual toggle via the sun/moon icon in the header
- Theme preference is saved to localStorage
- Smooth transitions between themes

## Responsive Design

The UI is fully responsive with:

- Mobile-first approach
- Collapsible sidebar on mobile devices
- Adaptive card layouts
- Touch-friendly interactions
- Optimized for all screen sizes

## API Integration

The application communicates with a .NET backend API for:

- Task CRUD operations
- Tag management
- User details management

All API calls are handled through the service layer in `src/services/api.ts`.

## Development

### Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run lint` - Run ESLint
- `npm run preview` - Preview production build

### Code Style

The project follows modern React and TypeScript best practices:

- Functional components with hooks
- TypeScript for type safety
- Clean, modular code structure
- Responsive design patterns
- Material-UI styling system

## Troubleshooting

### The page is blank or showing a loading spinner

- Make sure the backend API is running at `http://localhost:7071`
- Check the browser console for any errors
- Verify the API URL in the `.env` file

### Theme not switching

- Clear your browser's localStorage
- Check browser console for errors

### Cards not displaying properly

- Ensure all dependencies are installed (`npm install`)
- Try clearing the browser cache

## License

This project is part of a task management system assignment.
