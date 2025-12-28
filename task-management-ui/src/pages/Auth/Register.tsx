import { useState, type FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import {
  Container,
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  IconButton,
  InputAdornment,
  CircularProgress,
  LinearProgress,
  useTheme,
} from '@mui/material';
import { Visibility, VisibilityOff, PersonAdd } from '@mui/icons-material';
import { useAuth } from '../../contexts/AuthContext';
import './Auth.scss';

export const Register = () => {
  const theme = useTheme();
  const navigate = useNavigate();
  const { register } = useAuth();

  const [formData, setFormData] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    fullName: '',
    telephone: '',
  });
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const getPasswordStrength = (password: string): number => {
    let strength = 0;
    if (password.length >= 8) strength += 25;
    if (/[a-z]/.test(password)) strength += 25;
    if (/[A-Z]/.test(password)) strength += 25;
    if (/[0-9]/.test(password)) strength += 15;
    if (/[\W_]/.test(password)) strength += 10;
    return Math.min(strength, 100);
  };

  const passwordStrength = getPasswordStrength(formData.password);

  const getStrengthColor = (strength: number): 'error' | 'warning' | 'success' => {
    if (strength < 40) return 'error';
    if (strength < 80) return 'warning';
    return 'success';
  };

  const validateForm = (): string | null => {
    if (formData.password !== formData.confirmPassword) {
      return 'Passwords do not match';
    }
    if (formData.password.length < 8) {
      return 'Password must be at least 8 characters';
    }
    if (!/[A-Z]/.test(formData.password)) {
      return 'Password must contain at least one uppercase letter';
    }
    if (!/[a-z]/.test(formData.password)) {
      return 'Password must contain at least one lowercase letter';
    }
    if (!/[0-9]/.test(formData.password)) {
      return 'Password must contain at least one number';
    }
    if (!/[\W_]/.test(formData.password)) {
      return 'Password must contain at least one special character';
    }
    return null;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');

    const validationError = validateForm();
    if (validationError) {
      setError(validationError);
      return;
    }

    setLoading(true);

    try {
      await register({
        email: formData.email,
        password: formData.password,
        fullName: formData.fullName,
        telephone: formData.telephone,
      });
      navigate('/');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Registration failed. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  return (
    <div className="auth-container" data-theme={theme.palette.mode}>
      <Container maxWidth="sm">
        <Box className="auth-box">
          <Card className="auth-card">
            <CardContent>
              <Box className="auth-header">
                <PersonAdd className="auth-icon" />
                <Typography variant="h4" component="h1" gutterBottom>
                  Create Account
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Sign up to get started with Task Management
                </Typography>
              </Box>

              {error && (
                <Box className="error-message">
                  <Typography color="error" variant="body2">
                    {error}
                  </Typography>
                </Box>
              )}

              <form onSubmit={handleSubmit}>
                <TextField
                  fullWidth
                  label="Full Name"
                  value={formData.fullName}
                  onChange={(e) => handleChange('fullName', e.target.value)}
                  required
                  margin="normal"
                  autoComplete="name"
                  autoFocus
                />

                <TextField
                  fullWidth
                  label="Email"
                  type="email"
                  value={formData.email}
                  onChange={(e) => handleChange('email', e.target.value)}
                  required
                  margin="normal"
                  autoComplete="email"
                />

                <TextField
                  fullWidth
                  label="Telephone"
                  value={formData.telephone}
                  onChange={(e) => handleChange('telephone', e.target.value)}
                  required
                  margin="normal"
                  autoComplete="tel"
                />

                <TextField
                  fullWidth
                  label="Password"
                  type={showPassword ? 'text' : 'password'}
                  value={formData.password}
                  onChange={(e) => handleChange('password', e.target.value)}
                  required
                  margin="normal"
                  autoComplete="new-password"
                  InputProps={{
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          onClick={() => setShowPassword(!showPassword)}
                          edge="end"
                        >
                          {showPassword ? <VisibilityOff /> : <Visibility />}
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                />

                {formData.password && (
                  <Box sx={{ mt: 1 }}>
                    <Typography variant="caption" color="text.secondary">
                      Password Strength
                    </Typography>
                    <LinearProgress
                      variant="determinate"
                      value={passwordStrength}
                      color={getStrengthColor(passwordStrength)}
                      sx={{ height: 6, borderRadius: 3 }}
                    />
                  </Box>
                )}

                <TextField
                  fullWidth
                  label="Confirm Password"
                  type={showConfirmPassword ? 'text' : 'password'}
                  value={formData.confirmPassword}
                  onChange={(e) => handleChange('confirmPassword', e.target.value)}
                  required
                  margin="normal"
                  autoComplete="new-password"
                  InputProps={{
                    endAdornment: (
                      <InputAdornment position="end">
                        <IconButton
                          onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                          edge="end"
                        >
                          {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                />

                <Button
                  type="submit"
                  fullWidth
                  variant="contained"
                  size="large"
                  disabled={loading}
                  sx={{ mt: 3, mb: 2 }}
                >
                  {loading ? <CircularProgress size={24} /> : 'Sign Up'}
                </Button>

                <Box className="auth-footer">
                  <Typography variant="body2">
                    Already have an account?{' '}
                    <Link to="/login" className="auth-link">
                      Sign In
                    </Link>
                  </Typography>
                </Box>
              </form>
            </CardContent>
          </Card>
        </Box>
      </Container>
    </div>
  );
};
