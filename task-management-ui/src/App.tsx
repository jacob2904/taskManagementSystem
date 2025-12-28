import { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { ThemeProvider, CssBaseline, Box, Toolbar } from '@mui/material';
import { useAppTheme } from './hooks/useTheme';
import { NotificationProvider } from './contexts/NotificationContext';
import { AuthProvider } from './contexts/AuthContext';
import { Header } from './components/Header/Header';
import { Sidebar } from './components/Sidebar/Sidebar';
import { ProtectedRoute } from './components/ProtectedRoute/ProtectedRoute';
import { DashboardTable } from './pages/DashboardTable/DashboardTable';
import { Tasks } from './pages/Tasks/Tasks';
import { Tags } from './pages/Tags/Tags';
import { Login } from './pages/Auth/Login';
import { Register } from './pages/Auth/Register';
import './App.scss';

function App() {
  const { theme, mode, toggleColorMode } = useAppTheme();
  const [mobileOpen, setMobileOpen] = useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <AuthProvider>
        <NotificationProvider>
          <Router>
            <Routes>
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route
                path="/*"
                element={
                  <ProtectedRoute>
                    <Box className="app-container" data-theme={mode}>
                      <Header
                        mode={mode}
                        toggleColorMode={toggleColorMode}
                        onMenuClick={handleDrawerToggle}
                      />
                      <Sidebar mobileOpen={mobileOpen} onClose={handleDrawerToggle} />
                      <Box component="main" className="app-main">
                        <Toolbar />
                        <Routes>
                          <Route path="/" element={<DashboardTable />} />
                          <Route path="/tasks" element={<Tasks />} />
                          <Route path="/tags" element={<Tags />} />
                        </Routes>
                      </Box>
                    </Box>
                  </ProtectedRoute>
                }
              />
            </Routes>
          </Router>
        </NotificationProvider>
      </AuthProvider>
    </ThemeProvider>
  );
}

export default App;
