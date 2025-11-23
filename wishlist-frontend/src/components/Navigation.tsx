import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import '../styles/Navigation.css';

export function Navigation() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <nav className="navigation">
      <div className="nav-container">
        <Link to="/" className="nav-logo">
          🎁 Wishlist
        </Link>

        <div className="nav-links">
          <Link to="/" className="nav-link">All Wishlists</Link>
          <Link to="/my-wishlist" className="nav-link">My Wishlist</Link>
          <Link to="/my-claims" className="nav-link">My Claims</Link>
        </div>

        <div className="nav-user">
          <span className="user-name">Hi, {user?.name}</span>
          <button onClick={handleLogout} className="btn-logout">
            Logout
          </button>
        </div>
      </div>
    </nav>
  );
}
