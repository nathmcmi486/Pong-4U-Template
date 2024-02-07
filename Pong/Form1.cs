/*
 * Description:     A basic PONG simulator
 * Author:           
 * Date:            
 */

#region libraries

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Media;

#endregion

namespace Pong
{
    public partial class Form1 : Form
    {
        #region global values

        //graphics objects for drawing
        int brushColor = 255;
        SolidBrush whiteBrush = new SolidBrush(Color.White);
        SolidBrush greyBrush = new SolidBrush(Color.LightGray);
        Font drawFont = new Font("Courier New", 10);

        // Sounds for game
        SoundPlayer scoreSound = new SoundPlayer(Properties.Resources.score);
        SoundPlayer collisionSound = new SoundPlayer(Properties.Resources.collision);

        //determines whether a key is being pressed or not
        Boolean wKeyDown, sKeyDown, upKeyDown, downKeyDown;

        // check to see if a new game can be started
        Boolean newGameOk = true;
        Boolean firstGame = true;

        //ball values
        Boolean ballMoveRight = true;
        Boolean ballMoveDown = true;
        Boolean fakeBallMoveRight = false;
        Boolean fakeBallMoveDown = false;
        const int BALL_SPEED = 4;
        const int BALL_WIDTH = 20;
        const int BALL_HEIGHT = 20; 
        Rectangle ball, fakeBall;

        //player values
        const int PADDLE_SPEED = 4;
        const int PADDLE_EDGE = 20;  // buffer distance between screen edge and paddle            
        const int PADDLE_WIDTH = 10;
        const int PADDLE_HEIGHT = 40;
        int ballChangeXSpeed;
        int ballChangeYSpeed;
        Rectangle player1, player2;

        //player and game scores
        int player1Score = 0;
        int player2Score = 0;
        int gameWinScore = 3;  // number of points needed to win game

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void ballResetDisplay()
        {
            startLabel.Text = "Ready...";
            startLabel.Visible = true;
            this.Refresh();
            Thread.Sleep(1500);
            startLabel.Text = "Go!";
            this.Refresh();
            Thread.Sleep(750);
            startLabel.Visible = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //check to see if a key is pressed and set is KeyDown value to true if it has
            switch (e.KeyCode)
            {
                case Keys.W:
                    wKeyDown = true;
                    break;
                case Keys.S:
                    sKeyDown = true;
                    break;
                case Keys.Up:
                    upKeyDown = true;
                    break;
                case Keys.Down:
                    downKeyDown = true;
                    break;
                //case Keys.Y:
                case Keys.Space:
                    if (newGameOk)
                    {
                        SetParameters();
                    }
                    break;
                case Keys.Escape:
                    if (newGameOk)
                    {
                        Close();
                    }
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //check to see if a key has been released and set its KeyDown value to false if it has
            switch (e.KeyCode)
            {
                case Keys.W:
                    wKeyDown = false;
                    break;
                case Keys.S:
                    sKeyDown = false;
                    break;
                case Keys.Up:
                    upKeyDown = false;
                    break;
                case Keys.Down:
                    downKeyDown = false;
                    break;
            }
        }

        /// <summary>
        /// sets the ball and paddle positions for game start
        /// </summary>
        private void SetParameters()
        {
            if (newGameOk)
            {
                player1Score = player2Score = 0;
                newGameOk = false;
                startLabel.Visible = false;
                gameUpdateLoop.Start();
            }

            if (firstGame == false)
            {
                brushColor = Convert.ToInt16(Convert.ToDouble(brushColor) / 2.2);
                whiteBrush = new SolidBrush(Color.FromArgb(brushColor, brushColor, brushColor));
            }

            // Make sure player scores are set to 0
            player1Score = 0;
            player2Score = 0;
            player1ScoreLabel.Text = "0";
            plaery2ScoreLabel.Text = "0";

            //player start positions
            player1 = new Rectangle(PADDLE_EDGE, this.Height / 2 - PADDLE_HEIGHT / 2, PADDLE_WIDTH, PADDLE_HEIGHT);
            player2 = new Rectangle(this.Width - PADDLE_EDGE - PADDLE_WIDTH, this.Height / 2 - PADDLE_HEIGHT / 2, PADDLE_WIDTH, PADDLE_HEIGHT);

            ball = new Rectangle(this.Width / 2, this.Height / 2, BALL_WIDTH, BALL_HEIGHT);
            fakeBall = ball;
        }

        /// <summary>
        /// This method is the game engine loop that updates the position of all elements
        /// and checks for collisions.
        /// </summary>
        private void gameUpdateLoop_Tick(object sender, EventArgs e)
        {

            if (player1Score == 3)
            {
                GameOver("Player 1 Won!");
                return;
            }
            else if (player2Score == 3)
            {
                GameOver("Player 2 Won!");
                return;
            }

            #region update ball position

            if (ballMoveDown)
            {
                ball.Y -= (BALL_SPEED + ballChangeYSpeed);
            } else
            {
                ball.Y += (BALL_SPEED + ballChangeYSpeed);
            }

            if (ballMoveRight)
            {
                ball.X += (BALL_SPEED + ballChangeXSpeed);
            } else
            {
                ball.X -= (BALL_SPEED + ballChangeXSpeed);
            }

            if (fakeBallMoveDown)
            {
                fakeBall.Y -= BALL_SPEED;
            }
            else
            {
                fakeBall.Y += BALL_SPEED;
            }

            if (fakeBallMoveRight)
            {
                fakeBall.X += BALL_SPEED;
            }
            else
            {
                fakeBall.X -= BALL_SPEED;
            }

            #endregion

            #region update paddle positions

            if (wKeyDown == true && player1.Y > 0)
            {
                // TODO create code to move player 1 up
                player1.Y -= BALL_SPEED + 1;
            } else if (sKeyDown == true && player1.Y < this.Height)
            {
                player1.Y += BALL_SPEED + 1;
            }

            if (upKeyDown == true && player2.Y > 0)
            {
                player2.Y -= 4;
            } else if (downKeyDown == true && player2.Y < this.Height)
            {
                player2.Y += 4;
            }

            #endregion

            #region ball collision with top and bottom lines

            if (ball.Y < 0) // if ball hits top line
            {
                ballMoveDown = false;

                // Stops ball from going down if it's speed it negative
                if (ballChangeXSpeed < 0)
                {
                    ballChangeXSpeed = ballChangeXSpeed * -1;
                }

                if (ballChangeYSpeed < 0)
                {
                    ballChangeYSpeed = ballChangeYSpeed * -1;
                }

                collisionSound.Play();
                // TODO play a collision sound
            }
            else if (ball.Y > this.Height - PADDLE_EDGE)
            {
                ballMoveDown = true;

                // Stops ball from going up if it's speed it negative
                if (ballChangeXSpeed < 0)
                {
                    ballChangeXSpeed = ballChangeXSpeed * -1;
                }

                if (ballChangeYSpeed < 0)
                {
                    ballChangeYSpeed = ballChangeYSpeed * -1;
                }

                collisionSound.Play();
                collisionSound.Stop();
            }

            if (ball.X < 0)
            {
                ballMoveRight = true;
                collisionSound.Play();
                collisionSound.Stop();
            } else if (ball.X > this.Width - PADDLE_EDGE)
            {
                ballMoveRight = false;
                collisionSound.Play();
                collisionSound.Stop();
            }

            if (fakeBall.Y < 0) // if ball hits top line
            {
                fakeBallMoveDown = false;
                collisionSound.Play();
                collisionSound.Stop();
                // TODO play a collision sound
            }
            else if (fakeBall.Y > this.Height - PADDLE_EDGE)
            {
                fakeBallMoveDown = true;
                collisionSound.Play();
                collisionSound.Stop();
            }

            if (fakeBall.X < 0)
            {
                fakeBallMoveRight = true;
                collisionSound.Play();
                collisionSound.Stop();
            }
            else if (fakeBall.X > this.Width - PADDLE_EDGE)
            {
                fakeBallMoveRight = false;
                collisionSound.Play();
                collisionSound.Stop();
            }

            #endregion

            #region ball collision with paddles

            if (player1.IntersectsWith(ball) || player2.IntersectsWith(ball))
            {
                Random rand = new Random();
                int changeX = rand.Next(-3, 4);
                int changeY = rand.Next(-3, 4);
                ballChangeXSpeed += changeX;
                ballChangeYSpeed += changeY;

                ballMoveDown = !ballMoveDown;
                ballMoveRight = !ballMoveRight;
                fakeBallMoveDown = !ballMoveDown;
                fakeBallMoveRight = !ballMoveRight;
                collisionSound.Play();
            }

            #endregion

            #region ball collision with side walls (point scored)

            if (ball.X < PADDLE_EDGE / 2)  // ball hits left wall logic
            {
                // TODO
                // --- play score sound
                // --- update player 2 score and display it to the label
                player2Score += 1;
                plaery2ScoreLabel.Text = player2Score.ToString();

                scoreSound.Play();

                ball.X = this.Width / 2;
                ball.Y = this.Height / 2;

                fakeBall.X = this.Width / 2;
                fakeBall.Y = this.Height / 2;
                ballChangeXSpeed = 0;
                ballChangeYSpeed = 0;

                if (player2Score != 3)
                {
                    ballResetDisplay();
                }

            } else if (ball.X > this.Width - PADDLE_EDGE * 2)
            {
                player1Score += 1;
                player1ScoreLabel.Text = player1Score.ToString();

                scoreSound.Play();

                ball.X = this.Width / 2;
                ball.Y = this.Height / 2;

                fakeBall.X = this.Width / 2;
                fakeBall.Y = this.Height / 2;
                ballChangeXSpeed = 0;
                ballChangeYSpeed = 0;

                if (player1Score != 3)
                {
                    ballResetDisplay();
                }
            }

            #endregion
            
            //refresh the screen, which causes the Form1_Paint method to run
            this.Refresh();
        }
        
        /// <summary>
        /// Displays a message for the winner when the game is over and allows the user to either select
        /// to play again or end the program
        /// </summary>
        /// <param name="winner">The player name to be shown as the winner</param>
        private void GameOver(string winner)
        {
            newGameOk = true;

            // TODO create game over logic
            if (player1Score == 2)
            {
                // winner = "Player 1 Won!";
                startLabel.Text = "Player 1 Won!";
            } else if (player2Score == 2)
            {
                // winner = "Player 2 Won!";
                startLabel.Text = "Player 2 won!";
            }

            startLabel.Text += "\nPress Press Space To Start or Esc to Exit";

            startLabel.Visible = true;

            this.Refresh();
            this.gameUpdateLoop.Stop();
            firstGame = false;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(whiteBrush, player1);
            e.Graphics.FillRectangle(whiteBrush, player2);

            e.Graphics.FillRectangle(greyBrush, fakeBall);
            e.Graphics.FillRectangle(whiteBrush, ball);
        }

    }
}
