const express = require('express');
const path = require('path');
const dotenv = require('dotenv');
const cors = require('cors');
const helmet = require('helmet');
const rateLimit = require('express-rate-limit');
const Stripe = require('stripe');

dotenv.config();

const app = express();
const stripe = Stripe(process.env.STRIPE_SECRET_KEY || 'sk_test_placeholder');

// Middleware
app.use(helmet({ contentSecurityPolicy: false }));
app.use(cors());
app.use(express.json());

// Rate Limiter
const limiter = rateLimit({
  windowMs: 15 * 60 * 1000,
  max: 100
});
app.use(limiter);

// Serve Static Frontend
app.use(express.static(path.join(__dirname, 'public')));

// API Endpoint 1: Content Generator
app.post('/api/generate', (req, res) => {
  const { platform, topic, tone } = req.body;

  if (!topic) {
    return res.status(400).json({ error: 'Topic is required' });
  }

  let generatedContent = '';

  if (platform === 'linkedin') {
    generatedContent = `🚀 Most people misunderstand ${topic}. Here is what experience has taught me:\n\n` +
      `1. Focus on the fundamentals, not the noise.\n` +
      `2. Consistency beats intensity every single time.\n` +
      `3. Execute quietly and let results make the noise.\n\n` +
      `What is your take on ${topic}? Let me know below. 👇\n\n#Growth #${topic.replace(/\s+/g, '')} #Business`;
  } else if (platform === 'twitter') {
    generatedContent = `🧵 How to master ${topic} (without wasting 100+ hours):\n\n` +
      `1/ Stop overcomplicating the starting process.\n` +
      `2/ Build in public and document the journey.\n` +
      `3/ Double down on what works, eliminate the rest.\n\n` +
      `If you found this useful, RT the first tweet to share! 🔁`;
  } else {
    generatedContent = `📌 ${tone || 'Driven'} professional obsessed with ${topic}. Building scalable solutions and helping founders grow. Let's connect! ⚡`;
  }

  res.json({
    platform,
    content: generatedContent,
    timestamp: new Date()
  });
});

// API Endpoint 2: Stripe Paywall Checkout
app.post('/api/create-checkout-session', async (req, res) => {
  try {
    const session = await stripe.checkout.sessions.create({
      payment_method_types: ['card'],
      line_items: [
        {
          price_data: {
            currency: 'usd',
            product_data: {
              name: 'SocialPulse AI - Lifetime Access',
              description: 'Unlock unlimited AI social post & bio generations.',
            },
            unit_amount: 499, // $4.99 USD
          },
          quantity: 1,
        },
      ],
      mode: 'payment',
      success_url: `${req.headers.origin || 'http://localhost:10000'}?payment=success`,
      cancel_url: `${req.headers.origin || 'http://localhost:10000'}?payment=cancelled`,
    });

    res.json({ url: session.url });
  } catch (error) {
    console.error('Stripe Error:', error.message);
    res.status(500).json({ error: 'Failed to create payment session' });
  }
});

// Fallback to Web UI
app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

const PORT = process.env.PORT || 10000;
app.listen(PORT, () => {
  console.log(`SocialPulse AI running on port ${PORT}`);
});
