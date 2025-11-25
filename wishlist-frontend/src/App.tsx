import { BrowserRouter, Routes, Route, Navigate, Outlet } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import { Navigation } from './components/Navigation';
import { ProtectedRoute } from './components/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
import { DashboardPage } from './pages/DashboardPage';
import { WishlistPage } from './pages/WishlistPage';
import { MyWishlistPage } from './pages/MyWishlistPage';
import { MyClaimsPage } from './pages/MyClaimsPage';

function ProtectedLayout() {
  return (
    <ProtectedRoute>
      <div className="app-layout">
        <Navigation />
        <Outlet />
      </div>
    </ProtectedRoute>
  );
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          
          <Route element={<ProtectedLayout />}>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/wishlist/:userId" element={<WishlistPage />} />
            <Route path="/my-wishlist" element={<MyWishlistPage />} />
            <Route path="/my-claims" element={<MyClaimsPage />} />
          </Route>
          
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;
