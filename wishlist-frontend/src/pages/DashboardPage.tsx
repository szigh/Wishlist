import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import type { User } from '../types';
import { apiClient } from '../services/api';
import '../styles/Dashboard.css';

export function DashboardPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      const data = await apiClient.getUsers();
      setUsers(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load users');
    } finally {
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return <div className="loading">Loading wishlists...</div>;
  }

  if (error) {
    return <div className="error-message">{error}</div>;
  }

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1>Everyone's Wishlists</h1>
        <p>Browse and claim gifts to buy for others</p>
      </div>

      <div className="users-grid">
        {users.map(user => (
          <Link key={user.id} to={`/wishlist/${user.id}`} className="user-card">
            <div className="user-avatar">{user.name[0].toUpperCase()}</div>
            <div className="user-info">
              <h3>{user.name}</h3>
              <p>View wishlist →</p>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
