window.AppContext = {
    apiAddress: '@Configuration["ApiSettings:BaseUrl"]',
    token: '@User.FindFirst("Token")?.Value' // یا هر روشی که توکن را می‌گیری
};
