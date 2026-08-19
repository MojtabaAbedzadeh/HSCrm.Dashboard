window.AppContext = {
    apiAddress: '@Configuration["ApiSettings:BaseUrl"]',
    token: '@User.FindFirst("Token")?.Value'
};
