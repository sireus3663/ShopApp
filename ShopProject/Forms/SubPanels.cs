using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ShopProject.Db;
using ShopProject.Models;
using ShopProject.Services;

namespace ShopProject.Forms
{
    public interface IRefreshable
    {
        void Refresh();
    }

    public class CartPanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private Label lblTitle, lblTotal, lblEmpty;
        private ListView listCart;
        private Button btnClearCart, btnCheckout;

        public CartPanel(AuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = Color.FromArgb(245, 247, 250);

            lblTitle = new Label
            {
                Text = "\u041A\u043E\u0440\u0437\u0438\u043D\u0430",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(25, 25),
                AutoSize = true
            };

            listCart = new ListView
            {
                Location = new Point(25, 70),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };
            listCart.Columns.Add("\u0422\u043E\u0432\u0430\u0440", 300);
            listCart.Columns.Add("\u041A\u043E\u043B-\u0432\u043E", 80);
            listCart.Columns.Add("\u0426\u0435\u043D\u0430 \u0437\u0430 \u0448\u0442.", 120);
            listCart.Columns.Add("\u0421\u0443\u043C\u043C\u0430", 120);
            listCart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            lblEmpty = new Label
            {
                Text = "\u041A\u043E\u0440\u0437\u0438\u043D\u0430 \u043F\u0443\u0441\u0442\u0430",
                Font = new Font("Segoe UI", 13),
                ForeColor = Color.Gray,
                Location = new Point(25, 200),
                AutoSize = true,
                Visible = false
            };

            lblTotal = new Label
            {
                Text = "\u0418\u0442\u043E\u0433\u043E: 0 \u20BD",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(25, 520),
                AutoSize = true
            };

            btnClearCart = new Button
            {
                Text = "\u041E\u0447\u0438\u0441\u0442\u0438\u0442\u044C \u043A\u043E\u0440\u0437\u0438\u043D\u0443",
                Location = new Point(25, 560),
                Size = new Size(180, 38),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearCart.FlatAppearance.BorderSize = 0;
            btnClearCart.Click += (s, e) => ClearCart();

            btnCheckout = new Button
            {
                Text = "\u2713 \u041E\u0444\u043E\u0440\u043C\u0438\u0442\u044C \u0437\u0430\u043A\u0430\u0437",
                Location = new Point(220, 560),
                Size = new Size(200, 38),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += (s, e) => Checkout();

            Controls.AddRange(new Control[]
            {
                lblTitle, listCart, lblEmpty, lblTotal, btnClearCart, btnCheckout
            });

            Resize += (s, e) =>
            {
                listCart.Size = new Size(Width - 50, Height - 200);
                lblTotal.Location = new Point(25, Height - 120);
                btnClearCart.Location = new Point(25, Height - 75);
                btnCheckout.Location = new Point(220, Height - 75);
            };
        }

        public new void Refresh()
        {
            listCart.Items.Clear();
            var user = _authService.currentUser;
            if (user == null) { lblEmpty.Visible = true; return; }

            try
            {
                var cartRepo = new CartRepository(_context);
                var items = cartRepo.GetByUser(user.Id);

                if (!items.Any()) { lblEmpty.Visible = true; lblTotal.Text = "\u0418\u0442\u043E\u0433\u043E: 0 \u20BD"; return; }
                lblEmpty.Visible = false;

                decimal total = 0;
                foreach (var item in items)
                {
                    var product = _context.products.Find(item.ProductId);
                    if (product == null) continue;

                    decimal sum = product.Price * item.Count;
                    total += sum;

                    var row = new ListViewItem(product.Name);
                    row.SubItems.Add(item.Count.ToString());
                    row.SubItems.Add($"{product.Price:N0} \u20BD");
                    row.SubItems.Add($"{product.Price * item.Count:N0} \u20BD");
                    row.Tag = item;
                    listCart.Items.Add(row);
                }
                lblTotal.Text = $"\u0418\u0442\u043E\u0433\u043E: {total:N0} \u20BD";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"\u041E\u0448\u0438\u0431\u043A\u0430 \u0437\u0430\u0433\u0440\u0443\u0437\u043A\u0438 \u043A\u043E\u0440\u0437\u0438\u043D\u044B: {ex.Message}", "\u041E\u0448\u0438\u0431\u043A\u0430",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearCart()
        {
            var user = _authService.currentUser;
            if (user == null) return;
            var cartRepo = new CartRepository(_context);
            var items = cartRepo.GetByUser(user.Id);
            foreach (var item in items) cartRepo.Delete(item.Id);
            cartRepo.Save();
            Refresh();
        }

        private void Checkout()
        {
            MessageBox.Show("\u0417\u0430\u043A\u0430\u0437 \u043E\u0444\u043E\u0440\u043C\u043B\u0435\u043D!\n\n\u041F\u043E\u0434\u043A\u043B\u044E\u0447\u0438\u0442\u0435 OrderService \u0434\u043B\u044F \u043F\u043E\u043B\u043D\u043E\u0439 \u043E\u0431\u0440\u0430\u0431\u043E\u0442\u043A\u0438.",
                "\u0417\u0430\u043A\u0430\u0437", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public class FavoritesPanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private readonly AppDbContext _context;
        private Label lblTitle, lblEmpty;
        private FlowLayoutPanel flowFavorites;

        public FavoritesPanel(AuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = Color.FromArgb(245, 247, 250);

            lblTitle = new Label
            {
                Text = "\u0418\u0437\u0431\u0440\u0430\u043D\u043D\u043E\u0435",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(25, 25),
                AutoSize = true
            };

            flowFavorites = new FlowLayoutPanel
            {
                Location = new Point(25, 70),
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            flowFavorites.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            lblEmpty = new Label
            {
                Text = "\u2764  \u0417\u0434\u0435\u0441\u044C \u043F\u043E\u043A\u0430 \u043D\u0438\u0447\u0435\u0433\u043E \u043D\u0435\u0442.\n\u0414\u043E\u0431\u0430\u0432\u043B\u044F\u0439\u0442\u0435 \u0442\u043E\u0432\u0430\u0440\u044B \u0432 \u0438\u0437\u0431\u0440\u0430\u043D\u043D\u043E\u0435!",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray,
                Location = new Point(25, 120),
                AutoSize = true,
                Visible = false
            };

            Controls.AddRange(new Control[] { lblTitle, flowFavorites, lblEmpty });
            Resize += (s, e) =>
                flowFavorites.Size = new Size(Width - 50, Height - 80);
        }

        public new void Refresh()
        {
            flowFavorites.Controls.Clear();
            var user = _authService.currentUser;
            if (user == null) { lblEmpty.Visible = true; return; }

            try
            {
                var favRepo = new FavoriteRepository(_context);
                var favIds = favRepo.GetByUser(user.Id).Select(f => f.ProductId).ToList();

                if (!favIds.Any()) { lblEmpty.Visible = true; return; }
                lblEmpty.Visible = false;

                foreach (var id in favIds)
                {
                    var product = _context.products.Find(id);
                    if (product == null) continue;
                    flowFavorites.Controls.Add(MakeFavCard(product, favRepo, user.Id));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"\u041E\u0448\u0438\u0431\u043A\u0430: {ex.Message}", "\u041E\u0448\u0438\u0431\u043A\u0430",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Panel MakeFavCard(Product product, FavoriteRepository favRepo, Guid userId)
        {
            var card = new Panel
            {
                Size = new Size(220, 120),
                BackColor = Color.White,
                Margin = new Padding(6)
            };
            card.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(220, 220, 220)), 0, 0, card.Width - 1, card.Height - 1);

            var lblName = new Label
            {
                Text = product.Name,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(200, 40),
                AutoEllipsis = true
            };

            var lblPrice = new Label
            {
                Text = $"{product.Price:N0} \u20BD",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(37, 99, 235),
                Location = new Point(10, 55),
                AutoSize = true
            };

            var btnRemove = new Button
            {
                Text = "\u2715 \u0423\u0431\u0440\u0430\u0442\u044C",
                Location = new Point(10, 85),
                Size = new Size(90, 26),
                Font = new Font("Segoe UI", 8),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, e) =>
            {
                var item = favRepo.GetFavoriteItem(userId, product.Id);
                if (item != null) { favRepo.Delete(item.Id); favRepo.Save(); }
                Refresh();
            };

            var btnCart = new Button
            {
                Text = "\uD83D\uDED2",
                Location = new Point(110, 85),
                Size = new Size(50, 26),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCart.FlatAppearance.BorderSize = 0;
            btnCart.Click += (s, e) =>
            {
                var cartRepo = new CartRepository(_context);
                var existing = cartRepo.GetCartItem(userId, product.Id);
                if (existing != null) existing.Count++;
                else cartRepo.Add(new Cart { Id = Guid.NewGuid(), UserId = userId, ProductId = product.Id, Count = 1 });
                cartRepo.Save();
                MessageBox.Show($"\u00AB{product.Name}\u00BB \u0434\u043E\u0431\u0430\u0432\u043B\u0435\u043D \u0432 \u043A\u043E\u0440\u0437\u0438\u043D\u0443!", "\u041A\u043E\u0440\u0437\u0438\u043D\u0430",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            card.Controls.AddRange(new Control[] { lblName, lblPrice, btnRemove, btnCart });
            return card;
        }
    }

    public class ProfilePanel : Panel, IRefreshable
    {
        private readonly AuthService _authService;
        private Label lblTitle, lblName, lblEmail, lblRole, lblBalance,
                      lblNameVal, lblEmailVal, lblRoleVal, lblBalanceVal,
                      lblStats, lblOrdersCount;
        private Button btnEditName, btnChangePassword;
        private GroupBox grpInfo, grpStats;

        public ProfilePanel(AuthService authService)
        {
            _authService = authService;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = Color.FromArgb(245, 247, 250);

            lblTitle = new Label
            {
                Text = "\u041B\u0438\u0447\u043D\u044B\u0439 \u043A\u0430\u0431\u0438\u043D\u0435\u0442",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(25, 25),
                AutoSize = true
            };

            grpInfo = new GroupBox
            {
                Text = "\u041C\u043E\u0438 \u0434\u0430\u043D\u043D\u044B\u0435",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(25, 65),
                Size = new Size(420, 220),
                BackColor = Color.White
            };

            void AddInfoRow(string labelText, ref Label lbl, ref Label val, int y)
            {
                lbl = new Label { Text = labelText, Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(15, y), Size = new Size(120, 24) };
                val = new Label { Text = "\u2014", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.FromArgb(30, 30, 30), Location = new Point(140, y), Size = new Size(260, 24) };
                grpInfo.Controls.Add(lbl);
                grpInfo.Controls.Add(val);
            }

            AddInfoRow("\u0418\u043C\u044F:", ref lblName, ref lblNameVal, 35);
            AddInfoRow("Email:", ref lblEmail, ref lblEmailVal, 70);
            AddInfoRow("\u0420\u043E\u043B\u044C:", ref lblRole, ref lblRoleVal, 105);
            AddInfoRow("\u0411\u0430\u043B\u0430\u043D\u0441:", ref lblBalance, ref lblBalanceVal, 140);

            btnEditName = new Button
            {
                Text = "\u270F \u0418\u0437\u043C\u0435\u043D\u0438\u0442\u044C \u0438\u043C\u044F",
                Location = new Point(15, 175),
                Size = new Size(160, 32),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEditName.FlatAppearance.BorderSize = 0;
            btnEditName.Click += BtnEditName_Click;

            btnChangePassword = new Button
            {
                Text = "\uD83D\uDD12 \u0421\u043C\u0435\u043D\u0438\u0442\u044C \u043F\u0430\u0440\u043E\u043B\u044C",
                Location = new Point(185, 175),
                Size = new Size(160, 32),
                Font = new Font("Segoe UI", 9),
                BackColor = Color.FromArgb(100, 116, 139),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnChangePassword.FlatAppearance.BorderSize = 0;
            btnChangePassword.Click += BtnChangePassword_Click;

            grpInfo.Controls.AddRange(new Control[] { btnEditName, btnChangePassword });

            grpStats = new GroupBox
            {
                Text = "\u0421\u0442\u0430\u0442\u0438\u0441\u0442\u0438\u043A\u0430",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(25, 305),
                Size = new Size(420, 100),
                BackColor = Color.White
            };

            lblOrdersCount = new Label
            {
                Text = "\u0417\u0430\u043A\u0430\u0437\u043E\u0432: \u2014",
                Font = new Font("Segoe UI", 11),
                Location = new Point(15, 35),
                AutoSize = true
            };
            grpStats.Controls.Add(lblOrdersCount);

            Controls.AddRange(new Control[] { lblTitle, grpInfo, grpStats });
        }

        public new void Refresh()
        {
            var user = _authService.currentUser;
            if (user == null)
            {
                lblNameVal.Text = "\u041D\u0435 \u0430\u0432\u0442\u043E\u0440\u0438\u0437\u043E\u0432\u0430\u043D";
                lblEmailVal.Text = "\u2014";
                lblRoleVal.Text = "\u2014";
                lblBalanceVal.Text = "\u2014";
                return;
            }

            lblNameVal.Text = user.Name;
            lblEmailVal.Text = user.Email;
            lblRoleVal.Text = user.Role.ToString();
            lblBalanceVal.Text = $"{user.Balance:N0} \u20BD";
        }

        private void BtnEditName_Click(object sender, EventArgs e)
        {
            var user = _authService.currentUser;
            if (user == null) return;

            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "\u0412\u0432\u0435\u0434\u0438\u0442\u0435 \u043D\u043E\u0432\u043E\u0435 \u0438\u043C\u044F:", "\u0418\u0437\u043C\u0435\u043D\u0438\u0442\u044C \u0438\u043C\u044F", user.Name);

            if (!string.IsNullOrWhiteSpace(input) && input != user.Name)
            {
                user.Name = input;
                lblNameVal.Text = input;
                MessageBox.Show("\u0418\u043C\u044F \u043E\u0431\u043D\u043E\u0432\u043B\u0435\u043D\u043E!", "\u0413\u043E\u0442\u043E\u0432\u043E",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnChangePassword_Click(object sender, EventArgs e)
        {
            MessageBox.Show("\u0424\u0443\u043D\u043A\u0446\u0438\u044F \u0441\u043C\u0435\u043D\u044B \u043F\u0430\u0440\u043E\u043B\u044F \u0431\u0443\u0434\u0435\u0442 \u0434\u043E\u0431\u0430\u0432\u043B\u0435\u043D\u0430.\n(\u041F\u043E\u0434\u043A\u043B\u044E\u0447\u0438\u0442\u044C UserService.ChangePassword)",
                "\u0412 \u0440\u0430\u0437\u0440\u0430\u0431\u043E\u0442\u043A\u0435", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
