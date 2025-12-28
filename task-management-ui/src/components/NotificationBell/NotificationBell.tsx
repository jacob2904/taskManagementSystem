import { useState } from "react";
import {
  IconButton,
  Badge,
  Menu,
  MenuItem,
  Typography,
  Box,
  Divider,
  Button,
  Chip,
} from "@mui/material";
import {
  Notifications as NotificationsIcon,
  NotificationsActive,
  Close as CloseIcon,
} from "@mui/icons-material";
import { useNotificationContext } from "../../contexts/NotificationContext";
import { formatIsraelDateTime } from "../../utils/timezone";

export const NotificationBell = () => {
  const {
    notifications,
    isConnected,
    clearNotifications,
    removeNotificationByTaskId,
    requestNotificationPermission,
  } = useNotificationContext();

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleClearAll = () => {
    clearNotifications();
    handleClose();
  };

  const handleEnableNotifications = async () => {
    await requestNotificationPermission();
  };

  return (
    <>
      <IconButton
        color="inherit"
        onClick={handleClick}
        aria-label="notifications"
      >
        <Badge badgeContent={notifications.length} color="error">
          {isConnected ? <NotificationsActive /> : <NotificationsIcon />}
        </Badge>
      </IconButton>

      <Menu
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        PaperProps={{
          sx: {
            width: 360,
            maxHeight: 480,
          },
        }}
      >
        <Box
          sx={{
            p: 2,
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Typography variant="h6">
            Notifications
            {isConnected && (
              <Chip
                label="Connected"
                size="small"
                color="success"
                sx={{ ml: 1 }}
              />
            )}
          </Typography>
          {notifications.length > 0 && (
            <Button size="small" onClick={handleClearAll}>
              Clear All
            </Button>
          )}
        </Box>

        <Divider />

        {notifications.length === 0 ? (
          <Box sx={{ p: 3, textAlign: "center" }}>
            <Typography color="text.secondary">No notifications yet</Typography>
            {Notification.permission !== "granted" && (
              <Button
                size="small"
                onClick={handleEnableNotifications}
                sx={{ mt: 2 }}
              >
                Enable Browser Notifications
              </Button>
            )}
          </Box>
        ) : (
          notifications.map((notification, index) => (
            <MenuItem
              key={notification.taskId}
              sx={{
                flexDirection: "column",
                alignItems: "flex-start",
                py: 1.5,
                borderBottom: index < notifications.length - 1 ? 1 : 0,
                borderColor: "divider",
              }}
            >
              <Box
                sx={{
                  display: "flex",
                  justifyContent: "space-between",
                  width: "100%",
                }}
              >
                <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                  {notification.taskTitle}
                </Typography>
                <IconButton
                  size="small"
                  onClick={(e) => {
                    e.stopPropagation();
                    removeNotificationByTaskId(notification.taskId);
                  }}
                >
                  <CloseIcon fontSize="small" />
                </IconButton>
              </Box>
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{ mt: 0.5 }}
              >
                {notification.message}
              </Typography>
              <Typography
                variant="caption"
                color="text.secondary"
                sx={{ mt: 0.5 }}
              >
                Due: {formatIsraelDateTime(notification.dueDate)}
              </Typography>
            </MenuItem>
          ))
        )}
      </Menu>
    </>
  );
};
