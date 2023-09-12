## To test stripe locally
- install [Stripe cli](https://stripe.com/docs/stripe-cli#install)
- and login with `stripe login`
- then run `listen --forward-to https://localhost:7227/api/payments/webhook` to start listening to events from stripe
- Call with postman `/api/Payments/intent`