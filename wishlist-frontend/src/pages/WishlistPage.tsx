import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import type { UserWithWishlist, Gift } from '../types';
import { apiClient } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import '../styles/Wishlist.css';

export function WishlistPage() {
  const { userId } = useParams<{ userId: string }>();
  const { user: currentUser } = useAuth();
  const [userWithWishlist, setUserWithWishlist] = useState<UserWithWishlist | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (userId) {
      loadWishlist(parseInt(userId));
    }
  }, [userId]);

  const loadWishlist = async (id: number) => {
    try {
      const data = await apiClient.getUserWishlist(id);
      setUserWithWishlist(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load wishlist');
    } finally {
      setIsLoading(false);
    }
  };

  const handleClaimGift = async (gift: Gift) => {
    if (!confirm(`Do you want to claim "${gift.title}"? This means you'll buy it for ${userWithWishlist?.name}.`)) {
      return;
    }

    if (!currentUser) return;

    try {
      await apiClient.claimGift({ giftId: gift.id, volunteerUserId: currentUser.id });
      // Reload wishlist to see updated status
      if (userId) {
        await loadWishlist(parseInt(userId));
      }
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to claim gift');
    }
  };

  if (isLoading) {
    return <div className="loading">Loading wishlist...</div>;
  }

  if (error) {
    return <div className="error-message">{error}</div>;
  }

  if (!userWithWishlist) {
    return <div className="error-message">Wishlist not found</div>;
  }

  const isOwnWishlist = currentUser?.id === userWithWishlist.id;

  return (
    <div className="wishlist-page">
      <div className="wishlist-header">
        <h1>{userWithWishlist.name}'s Wishlist</h1>
        {isOwnWishlist ? (
          <p>This is your wishlist. Go to "My Wishlist" to edit it.</p>
        ) : (
          <p>Click "I'll buy this" to claim a gift</p>
        )}
      </div>

      {userWithWishlist.gifts.length === 0 ? (
        <div className="empty-state">
          <p>{userWithWishlist.name} hasn't added any gifts yet.</p>
        </div>
      ) : (
        <div className="gifts-grid">
          {userWithWishlist.gifts.map(gift => (
            <div key={gift.id} className={`gift-card ${gift.isTaken ? 'claimed' : ''}`}>
              <div className="gift-header">
                <h3>{gift.title}</h3>
                {gift.isTaken && <span className="claimed-badge">Claimed</span>}
              </div>
              
              {gift.description && <p className="gift-description">{gift.description}</p>}
              
              {gift.category && <span className="gift-category">{gift.category}</span>}
              
              {gift.link && (
                <a href={gift.link} target="_blank" rel="noopener noreferrer" className="gift-link">
                  View Item →
                </a>
              )}

              {!isOwnWishlist && !gift.isTaken && (
                <button
                  onClick={() => handleClaimGift(gift)}
                  className="btn-claim"
                >
                  I'll buy this 🎁
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
