import { useState, useEffect, useCallback } from "react";
import type { FormEvent } from "react";
import type { Gift, GiftCreate } from "../types";
import { apiClient } from "../services/api";
import { useAuth } from "../contexts/AuthContext";
import "../styles/MyWishlist.css";

export function MyWishlistPage() {
  const { user } = useAuth();
  const [gifts, setGifts] = useState<Gift[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [showAddForm, setShowAddForm] = useState(false);
  const [showModifyForm, setShowModifyForm] = useState(false);
  const [giftToModify, setGiftToModify] = useState<Gift | null>(null);

  // Form state
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [link, setLink] = useState("");
  const [category, setCategory] = useState("");

  const loadMyGifts = useCallback(async () => {
    if (!user?.id) return;

    try {
      const userWithWishlist = await apiClient.getUserWishlist(user.id);
      setGifts(userWithWishlist.gifts);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load gifts");
    } finally {
      setIsLoading(false);
    }
  }, [user?.id]);

  useEffect(() => {
    loadMyGifts();
  }, [loadMyGifts]);

  const handleAddGift = async (e: FormEvent) => {
    e.preventDefault();

    const newGift: GiftCreate = {
      title,
      description: description || undefined,
      link: link || undefined,
      category: category || undefined,
    };

    try {
      await apiClient.createGift(newGift);
      setTitle("");
      setDescription("");
      setLink("");
      setCategory("");
      setShowAddForm(false);
      setShowModifyForm(false);
      await loadMyGifts();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Failed to add gift");
    }
  };

  const handleDeleteGift = async (
    id: number,
    title: string,
    confirmDelete: boolean = true
  ) => {
    if (confirmDelete && !confirm(`Delete "${title}"?`)) {
      return;
    }

    try {
      await apiClient.deleteGift(id);
      await loadMyGifts();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Failed to delete gift");
    }
  };

  if (isLoading) {
    return <div className="loading">Loading your wishlist...</div>;
  }

  return (
    <div className="my-wishlist-page">
      <div className="page-header">
        <h1>My Wishlist</h1>
        <button
          onClick={() => setShowAddForm(!showAddForm)}
          className="btn-primary"
        >
          {showAddForm ? "Cancel" : "+ Add Gift"}
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {showAddForm && (
        <form onSubmit={handleAddGift} className="add-gift-form">
          <h3>Add New Gift</h3>

          <div className="form-group">
            <label htmlFor="title">Title *</label>
            <input
              id="title"
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              placeholder="e.g., Wireless Headphones"
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Any specific details..."
              rows={3}
            />
          </div>

          <div className="form-group">
            <label htmlFor="link">Link</label>
            <input
              id="link"
              type="url"
              value={link}
              onChange={(e) => setLink(e.target.value)}
              placeholder="https://..."
            />
          </div>

          <div className="form-group">
            <label htmlFor="category">Category</label>
            <input
              id="category"
              type="text"
              value={category}
              onChange={(e) => setCategory(e.target.value)}
              placeholder="e.g., Electronics, Books, Clothing"
            />
          </div>

          <button type="submit" className="btn-primary">
            Add to My Wishlist
          </button>
        </form>
      )}

      {gifts.length === 0 ? (
        <div className="empty-state">
          <p>Your wishlist is empty. Add some gifts to get started!</p>
        </div>
      ) : (
        <div className="gifts-grid">
          {gifts.map((gift) => (
            <div
              key={gift.id}
              className={`gift-card ${gift.isTaken ? "claimed" : ""}`}
            >
              <div className="gift-header">
                <h3>{gift.title}</h3>
                <span className="gift-id">ID: {gift.id}</span>
                {gift.isTaken && (
                  <span className="claimed-badge">
                    Someone will buy this 🎉
                  </span>
                )}
                {!gift.isTaken && (
                  <button
                    onClick={() => {
                      setShowModifyForm(!showModifyForm);
                      setGiftToModify(gift);
                      // Initialize form state with current gift values
                      setTitle(gift.title);
                      setDescription(gift.description || "");
                      setLink(gift.link || "");
                      setCategory(gift.category || "");
                    }}
                    className="available-badge"
                  >
                    {showModifyForm ? "Cancel" : "Edit Gift"}
                  </button>
                )}
              </div>

              {gift.description && (
                <p className="gift-description">{gift.description}</p>
              )}

              {gift.category && (
                <span className="gift-category">{gift.category}</span>
              )}

              {gift.link && (
                <a
                  href={gift.link}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="gift-link"
                >
                  View Item →
                </a>
              )}

              <button
                onClick={() => handleDeleteGift(gift.id, gift.title)}
                className="btn-delete"
              >
                Delete
              </button>
            </div>
          ))}
        </div>
      )}

      {showModifyForm && (
        <form
          onSubmit={(e) => {
            handleAddGift(e);
            handleDeleteGift(giftToModify!.id, giftToModify!.title, false);
          }}
          className="add-gift-form"
        >
          <h3>ModifyGift</h3>

          <div className="form-group">
            <label htmlFor="title">Title *</label>
            <input
              id="title"
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              placeholder="e.g., Wireless Headphones"
            />
          </div>

          <div className="form-group">
            <label htmlFor="description">Description</label>
            <textarea
              id="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Any specific details..."
              rows={3}
            />
          </div>

          <div className="form-group">
            <label htmlFor="link">Link</label>
            <input
              id="link"
              type="url"
              value={link}
              onChange={(e) => setLink(e.target.value)}
              placeholder="https://..."
            />
          </div>

          <div className="form-group">
            <label htmlFor="category">Category</label>
            <input
              id="category"
              type="text"
              value={category}
              onChange={(e) => setCategory(e.target.value)}
              placeholder="e.g., Electronics, Books, Clothing"
            />
          </div>

          <button type="submit" className="btn-primary">
            Update gift
          </button>
        </form>
      )}
    </div>
  );
}
