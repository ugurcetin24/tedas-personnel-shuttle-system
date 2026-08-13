import AssignmentTurnedIn from '@mui/icons-material/AssignmentTurnedIn'
import Dashboard from '@mui/icons-material/Dashboard'
import DirectionsBus from '@mui/icons-material/DirectionsBus'
import DriveEta from '@mui/icons-material/DriveEta'
import Groups from '@mui/icons-material/Groups'
import Map from '@mui/icons-material/Map'
import UploadFile from '@mui/icons-material/UploadFile'
import {
  AppBar,
  Box,
  Divider,
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Stack,
  Toolbar,
  Typography,
} from '@mui/material'
import { NavLink, Outlet } from 'react-router'
import { HealthBadge } from '../components/HealthBadge'

const drawerWidth = 280

const navigationItems = [
  { label: 'Dashboard', path: '/', icon: <Dashboard /> },
  { label: 'Personeller', path: '/personnel', icon: <Groups /> },
  { label: 'Servisler', path: '/shuttles', icon: <DirectionsBus /> },
  { label: 'Şoförler', path: '/drivers', icon: <DriveEta /> },
  { label: 'Servis Atamaları', path: '/assignments', icon: <AssignmentTurnedIn /> },
  { label: 'Güzergâhlar', path: '/routes', icon: <Map /> },
  { label: 'Excel Aktarım', path: '/imports', icon: <UploadFile /> },
]

export function AppLayout() {
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      <Drawer
        variant="permanent"
        sx={{
          width: drawerWidth,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: drawerWidth,
            boxSizing: 'border-box',
            borderRightColor: 'divider',
            bgcolor: 'background.paper',
          },
        }}
      >
        <Stack spacing={0.5} sx={{ px: 3, py: 2.5 }}>
          <Typography variant="overline" color="primary.main" sx={{ fontWeight: 800 }}>
            TEDAŞ
          </Typography>
          <Typography variant="h6" sx={{ lineHeight: 1.15, fontWeight: 800 }}>
            Personel Servisi Atama Sistemi
          </Typography>
        </Stack>
        <Divider />
        <List sx={{ px: 1.5, py: 2 }}>
          {navigationItems.map((item) => (
            <ListItemButton
              key={item.path}
              component={NavLink}
              to={item.path}
              end={item.path === '/'}
              sx={{
                borderRadius: 1,
                minHeight: 44,
                mb: 0.5,
                '&.active': {
                  bgcolor: 'primary.main',
                  color: 'primary.contrastText',
                  '& .MuiListItemIcon-root': {
                    color: 'primary.contrastText',
                  },
                },
              }}
            >
              <ListItemIcon sx={{ minWidth: 40 }}>{item.icon}</ListItemIcon>
              <ListItemText
                primary={item.label}
                slotProps={{
                  primary: {
                    sx: { fontSize: 14, fontWeight: 700 },
                  },
                }}
              />
            </ListItemButton>
          ))}
        </List>
      </Drawer>

      <Box component="main" sx={{ flexGrow: 1, minWidth: 0 }}>
        <AppBar
          position="sticky"
          color="inherit"
          elevation={0}
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Toolbar sx={{ justifyContent: 'space-between', gap: 2 }}>
            <Typography variant="h6" sx={{ fontWeight: 800 }}>
              TEDAŞ Personel Servisi Atama Sistemi
            </Typography>
            <HealthBadge />
          </Toolbar>
        </AppBar>
        <Box sx={{ p: 3 }}>
          <Outlet />
        </Box>
      </Box>
    </Box>
  )
}
