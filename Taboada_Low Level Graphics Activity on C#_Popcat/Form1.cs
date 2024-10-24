using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Taboada_Low_Level_Graphics_Activity_on_C__Popcat
{
    public partial class Form1 : Form
    {
        bool goup, godown, goleft, goright, isGameOver;
        int score, playerSpeed, ghostSpeed;
        Random rnd = new Random();

        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(625, 498);  
            InitializeGhostTimer();
            resetGame();
        }

        private void resetGame()
        {
            goup = godown = goleft = goright = false;
            score = 0;
            playerSpeed = 5;
            ghostSpeed = 3;
            isGameOver = false;

            popCat.Image = Properties.Resources.right;  // Set initial popcat image
            popCat.Location = new Point(8, 36);

            // Set initial ghost positions and images
            SetGhostPosition(ghostOne, 265, 410, "up");
            SetGhostPosition(ghostTwo, 6, 200, "left");
            SetGhostPosition(ghostThree, 565, 134, "down");

            foreach (Control x in this.Controls)
            {
                if (x is PictureBox && (string)x.Tag == "nibs")
                {
                    x.Visible = true;   // Make all nibs visible
                    x.Enabled = true;   // Mark all nibs as "uneaten"
                }
            }

            txtScore.Text = "Score: 0";
            resetButton.Visible = false;

            gameTimer.Start();
            ghostTimer.Start();
            this.Focus();
        }

        private void SetGhostPosition(PictureBox ghost, int x, int y, string direction)
        {
            ghost.Location = new Point(x, y);
            ghost.Tag = direction;  // Store current direction
            UpdateGhostImage(ghost, direction);  // Set initial ghost image
        }

        private void keyisdown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) goup = true;
            if (e.KeyCode == Keys.Down) godown = true;
            if (e.KeyCode == Keys.Left) goleft = true;
            if (e.KeyCode == Keys.Right) goright = true;
        }

        private void keyisup(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) goup = false;
            if (e.KeyCode == Keys.Down) godown = false;
            if (e.KeyCode == Keys.Left) goleft = false;
            if (e.KeyCode == Keys.Right) goright = false;

            if (e.KeyCode == Keys.Enter && isGameOver) resetGame();
        }

        private void mainGameTimer(object sender, EventArgs e)
        {
            txtScore.Text = "Score: " + score;

            // Handle Popcat Movement with Boundary Check wowzers
            if (goleft && popCat.Left > 0)
            {
                popCat.Left -= playerSpeed;
                popCat.Image = Properties.Resources.left;
            }
            if (goright && popCat.Right < ClientSize.Width)
            {
                popCat.Left += playerSpeed;
                popCat.Image = Properties.Resources.right;
            }
            if (godown && popCat.Bottom < ClientSize.Height)
            {
                popCat.Top += playerSpeed;
                popCat.Image = Properties.Resources.downRight;
            }
            if (goup && popCat.Top > 0)
            {
                popCat.Top -= playerSpeed;
                popCat.Image = Properties.Resources.upRight;
            }

            // Collision Handling
            foreach (Control x in this.Controls)
            {
                if (x is PictureBox && (string)x.Tag == "nibs" && x.Visible)
                {
                    if (popCat.Bounds.IntersectsWith(x.Bounds))
                    {
                        score++;
                        x.Visible = false;  // Hide the nib permanently when eaten
                        x.Enabled = false;  
                    }
                }

                if (x is PictureBox && (string)x.Tag == "wall" && popCat.Bounds.IntersectsWith(x.Bounds))
                {
                    gameOver("You Lose!");
                }

                if (x is PictureBox && (string)x.Tag == "ghost" && popCat.Bounds.IntersectsWith(x.Bounds))
                {
                    gameOver("You Lose!");
                }
            }

            if (!this.Controls.OfType<PictureBox>().Any(p => (string)p.Tag == "nibs" && p.Visible))
            {
                gameOver("You Win!");
            }
        }

        private void InitializeGhostTimer()
        {
            ghostTimer.Interval = 100;
            ghostTimer.Tick += UpdateGhostPositions;
        }

        private void UpdateGhostPositions(object sender, EventArgs e)
        {
            if (!isGameOver)
            {
                MoveGhost(ghostOne);
                MoveGhost(ghostTwo);
                MoveGhost(ghostThree);

                if (popCat.Bounds.IntersectsWith(ghostOne.Bounds) ||
                    popCat.Bounds.IntersectsWith(ghostTwo.Bounds) ||
                    popCat.Bounds.IntersectsWith(ghostThree.Bounds))
                {
                    gameOver("You Lose!");
                }

                HandleGhostNibsInteraction();
            }
        }

        private void HandleGhostNibsInteraction()
        {
            // Check each nib to see if a ghost overlaps with it
            foreach (Control nib in this.Controls.OfType<PictureBox>().Where(p => (string)p.Tag == "nibs" && p.Enabled))
            {
                bool isAnyGhostOverlapping = ghostOne.Bounds.IntersectsWith(nib.Bounds) ||
                                             ghostTwo.Bounds.IntersectsWith(nib.Bounds) ||
                                             ghostThree.Bounds.IntersectsWith(nib.Bounds);

                nib.Visible = !isAnyGhostOverlapping;  // Hide nib only temporarily when ghosts overlap
            }
        }

        private void MoveGhost(PictureBox ghost)
        {
            Point originalPosition = ghost.Location;
            bool moved = false;

            switch (ghost.Tag.ToString())
            {
                case "up":
                    if (ghost.Top > 0) { ghost.Top -= ghostSpeed; moved = true; }
                    break;
                case "down":
                    if (ghost.Bottom < ClientSize.Height) { ghost.Top += ghostSpeed; moved = true; }
                    break;
                case "left":
                    if (ghost.Left > 0) { ghost.Left -= ghostSpeed; moved = true; }
                    break;
                case "right":
                    if (ghost.Right < ClientSize.Width) { ghost.Left += ghostSpeed; moved = true; }
                    break;
            }

            if (IsGhostCollidingWithWall(ghost) || !moved)
            {
                ghost.Location = originalPosition;
                ChooseNewDirection(ghost);
            }
        }

        private void ChooseNewDirection(PictureBox ghost)
        {
            string[] directions = { "up", "down", "left", "right" };
            string newDirection = directions[rnd.Next(directions.Length)];
            ghost.Tag = newDirection;
            UpdateGhostImage(ghost, newDirection);
        }

        private void UpdateGhostImage(PictureBox ghost, string direction)
        {
            switch (direction)
            {
                case "up": ghost.Image = Properties.Resources.ghostUp; break;
                case "down": ghost.Image = Properties.Resources.ghostDown; break;
                case "left": ghost.Image = Properties.Resources.ghostLeft; break;
                case "right": ghost.Image = Properties.Resources.ghostRight; break;
            }
        }

        private bool IsGhostCollidingWithWall(PictureBox ghost)
        {
            foreach (Control x in this.Controls)
            {
                if (x is PictureBox && (string)x.Tag == "wall" && ghost.Bounds.IntersectsWith(x.Bounds))
                {
                    return true;
                }
            }
            return false;
        }

        private void gameOver(string message)
        {
            isGameOver = true;
            gameTimer.Stop();
            ghostTimer.Stop();
            txtScore.Text = "Score: " + score + Environment.NewLine + message;
            resetButton.Visible = true;
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            gameTimer.Stop();
            ghostTimer.Stop();
            resetGame();
        }
    }
}
