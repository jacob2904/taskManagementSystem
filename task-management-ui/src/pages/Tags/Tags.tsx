import { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  Button,
  TextField,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
  Alert,
  Card,
  CardContent,
  Chip,
  Fade,
  Tooltip,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, Label as LabelIcon } from '@mui/icons-material';
import { tagService } from '../../services/api';
import type { Tag, CreateTagDto } from '../../types';
import './Tags.scss';

export const Tags = () => {
  const [tags, setTags] = useState<Tag[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedTag, setSelectedTag] = useState<Tag | null>(null);
  const [tagToDelete, setTagToDelete] = useState<number | null>(null);
  const [tagName, setTagName] = useState('');
  const [formError, setFormError] = useState('');

  useEffect(() => {
    loadTags();
  }, []);

  const loadTags = async () => {
    try {
      setLoading(true);
      const data = await tagService.getTags();
      setTags(data);
      setError(null);
    } catch (err) {
      setError('Failed to load tags');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenForm = (tag?: Tag) => {
    if (tag) {
      setSelectedTag(tag);
      setTagName(tag.name);
    } else {
      setSelectedTag(null);
      setTagName('');
    }
    setFormError('');
    setFormOpen(true);
  };

  const handleCloseForm = () => {
    setFormOpen(false);
    setSelectedTag(null);
    setTagName('');
    setFormError('');
  };

  const handleSubmit = async () => {
    if (!tagName.trim()) {
      setFormError('Tag name is required');
      return;
    }

    try {
      const tagDto: CreateTagDto = { name: tagName.trim() };
      if (selectedTag) {
        await tagService.updateTag(selectedTag.id, tagDto);
      } else {
        await tagService.createTag(tagDto);
      }
      await loadTags();
      handleCloseForm();
    } catch (err) {
      setFormError('Failed to save tag');
      console.error(err);
    }
  };

  const handleDeleteClick = (id: number) => {
    setTagToDelete(id);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (tagToDelete === null) return;
    try {
      await tagService.deleteTag(tagToDelete);
      await loadTags();
      setDeleteDialogOpen(false);
      setTagToDelete(null);
    } catch (err) {
      console.error('Error deleting tag:', err);
    }
  };

  if (loading) {
    return (
      <Box className="tags-page__loading">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box className="tags-page">
      <Box className="tags-page__header">
        <Box className="tags-page__header-content">
          <LabelIcon className="tags-page__header-icon" />
          <Box>
            <Typography variant="h4" className="tags-page__title">
              Tags
            </Typography>
            <Typography variant="body2" color="text.secondary" className="tags-page__subtitle">
              Organize and categorize your tasks
            </Typography>
          </Box>
        </Box>
        <Button 
          variant="contained" 
          startIcon={<AddIcon />} 
          onClick={() => handleOpenForm()}
          className="tags-page__add-button"
        >
          New Tag
        </Button>
      </Box>

      {error && (
        <Fade in>
          <Alert severity="error" className="tags-page__error" onClose={() => setError(null)}>
            {error}
          </Alert>
        </Fade>
      )}

      <Box className="tags-page__content">
        {tags.length === 0 ? (
          <Card className="tags-page__empty-card">
            <CardContent className="tags-page__empty">
              <LabelIcon className="tags-page__empty-icon" />
              <Typography variant="h6" className="tags-page__empty-title">
                No tags yet
              </Typography>
              <Typography variant="body2" color="text.secondary" className="tags-page__empty-subtitle">
                Create your first tag to start organizing tasks
              </Typography>
              <Button 
                variant="contained" 
                startIcon={<AddIcon />} 
                onClick={() => handleOpenForm()}
                sx={{ mt: 2 }}
              >
                Create Tag
              </Button>
            </CardContent>
          </Card>
        ) : (
          <Box className="tags-page__grid">
            {tags.map((tag, index) => (
              <Fade in key={tag.id} timeout={300 + index * 50}>
                <Card className="tags-page__tag-card">
                  <CardContent className="tags-page__tag-content">
                    <Chip 
                      label={tag.name} 
                      color="primary" 
                      icon={<LabelIcon />}
                      className="tags-page__chip" 
                    />
                    <Box className="tags-page__tag-actions">
                      <Tooltip title="Edit tag">
                        <IconButton 
                          size="small" 
                          onClick={() => handleOpenForm(tag)}
                          className="tags-page__action-button"
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete tag">
                        <IconButton 
                          size="small" 
                          onClick={() => handleDeleteClick(tag.id)}
                          className="tags-page__action-button tags-page__action-button--delete"
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </CardContent>
                </Card>
              </Fade>
            ))}
          </Box>
        )}
      </Box>

      <Dialog open={formOpen} onClose={handleCloseForm} maxWidth="xs" fullWidth>
        <DialogTitle>{selectedTag ? 'Edit Tag' : 'Create New Tag'}</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Tag Name"
            fullWidth
            value={tagName}
            onChange={(e) => setTagName(e.target.value)}
            error={!!formError}
            helperText={formError}
            onKeyPress={(e) => {
              if (e.key === 'Enter') {
                handleSubmit();
              }
            }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseForm}>Cancel</Button>
          <Button onClick={handleSubmit} variant="contained">
            {selectedTag ? 'Update' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Delete Tag</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete this tag? This action cannot be undone.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};
