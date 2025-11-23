import { useState, useEffect } from 'react';
import type { Volunteer } from '../types';
import { apiClient } from '../services/api';
import '../styles/MyClaims.css';

export function MyClaimsPage() {
  const [claims, setClaims] = useState<Volunteer[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadMyClaims();
  }, []);

  const loadMyClaims = async () => {
    try {
      const data = await apiClient.getVolunteers();
      setClaims(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load claims');
    } finally {
      setIsLoading(false);
    }
  };

  const handleUnclaim = async (claim: Volunteer) => {
    if (!confirm(`Unclaim "${claim.gift?.title}"? This will allow someone else to buy it.`)) {
      return;
    }

    try {
      await apiClient.unclaimGift(claim.id);
      await loadMyClaims();
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to unclaim gift');
    }
  };

  if (isLoading) {
    return <div className="loading">Loading your claims...</div>;
  }

  if (error) {
    return <div className="error-message">{error}</div>;
  }

  return (
    <div className="my-claims-page">
      <div className="page-header">
        <h1>Gifts I'm Buying</h1>
        <p>These are the gifts you've volunteered to purchase</p>
      </div>

      {claims.length === 0 ? (
        <div className="empty-state">
          <p>You haven't claimed any gifts yet. Browse others' wishlists to find something to buy!</p>
        </div>
      ) : (
        <div className="claims-grid">
          {claims.map(claim => (
            <div key={claim.id} className="claim-card">
              <h3>{claim.gift?.title}</h3>
              
              {claim.gift?.description && (
                <p className="claim-description">{claim.gift.description}</p>
              )}
              
              {claim.gift?.category && (
                <span className="claim-category">{claim.gift.category}</span>
              )}
              
              {claim.gift?.link && (
                <a 
                  href={claim.gift.link} 
                  target="_blank" 
                  rel="noopener noreferrer" 
                  className="claim-link"
                >
                  View Item →
                </a>
              )}

              <button
                onClick={() => handleUnclaim(claim)}
                className="btn-unclaim"
              >
                Unclaim Gift
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
