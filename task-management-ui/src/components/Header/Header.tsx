import { AppBar, Toolbar, Typography, IconButton, Box, Avatar, Menu, MenuItem, Tooltip } from '@mui/material';
import { Brightness4, Brightness7, Menu as MenuIcon, Logout } from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { NotificationBell } from '../NotificationBell/NotificationBell';

interface HeaderProps {
  mode: 'light' | 'dark';
  toggleColorMode: () => void;
  onMenuClick: () => void;
}

export const Header = ({ mode, toggleColorMode, onMenuClick }: HeaderProps) => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    logout();
    handleClose();
    navigate('/login');
  };

  return (
    <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
      <Toolbar>
        <IconButton
          color="inherit"
          edge="start"
          onClick={onMenuClick}
          sx={{ mr: 2, display: { sm: 'none' } }}
        >
          <MenuIcon />
        </IconButton>

        <Typography
          variant="h6"
          component="div"
          sx={{
            flexGrow: 1,
            cursor: 'pointer',
            fontWeight: 600,
          }}
          onClick={() => navigate('/')}
        >
          Task Management
        </Typography>

        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <NotificationBell />
          <IconButton onClick={toggleColorMode} color="inherit">
            {mode === 'dark' ? <Brightness7 /> : <Brightness4 />}
          </IconButton>
          
          {user && (
            <>
              <Tooltip title="Account">
                <IconButton onClick={handleMenu} sx={{ p: 0, ml: 1 }}>
                  <Avatar
                    sx={{
                      bgcolor: 'secondary.main',
                      width: 36,
                      height: 36,
                      fontSize: '1rem',
                    }}
                  >
                    {user.fullName.charAt(0).toUpperCase()}
                  </Avatar>
                </IconButton>
              </Tooltip>
              <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={handleClose}
                anchorOrigin={{
                  vertical: 'bottom',
                  horizontal: 'right',
                }}
                transformOrigin={{
                  vertical: 'top',
                  horizontal: 'right',
                }}
              >
                <Box sx={{ px: 2, py: 1.5, borderBottom: 1, borderColor: 'divider' }}>
                  <Typography variant="subtitle2" fontWeight={600}>
                    {user.fullName}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {user.email}
                  </Typography>
                </Box>
                <MenuItem onClick={handleLogout}>
                  <Logout sx={{ mr: 1.5 }} fontSize="small" />
                  Logout
                </MenuItem>
              </Menu>
            </>
          )}
        </Box>
      </Toolbar>
    </AppBar>
  );
};
