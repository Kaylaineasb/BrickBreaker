using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    //referencia a localização do jogador
    public Transform playerBar;
    //velocidade inicial que ela é lançada
    public float initialSpeed = 10f;
    //raio da bola
    public float radius = 0.2f;
    //ângulo amixo que a bola pode quicar ao atingir o player
    public float maxBounceAngle = 75f;

    //direcao e velocidade atual da bola
    private Vector2 velocity;
    //lançada ou não
    private bool isLaunched = false;

    //evita que a bola colida com o player multiplas vezes seguidas
    private float cooldownTimer = 0f;
    private const float collisionCooldown = 0.2f;
    //tamanho da area de colisão
    public Vector2 paddleCollisionSize = new Vector2(2f, 0.5f);

    private List<GameObject> m_allBricks;

    void Start()
    {
        ResetBall(); //estado inicial da bola
    }

    public void StartLevel(List<GameObject> bricks)
    {
        m_allBricks = bricks;
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (!isLaunched)
        {
            StickToPaddle();// bola presa na barra
            if (Input.GetMouseButtonDown(0)) LaunchBall();// se clicar com o botão, lança a bola
            return;
        }

        MoveBall();
        HandleWallCollisions();
        if (cooldownTimer <= 0f) HandlePaddleCollision();

        if (m_allBricks != null)
        {
            HandleBrickCollisions();
        }
    }

    //coloca a bola acima do centro da barra do jogador
    void StickToPaddle()
    {
        transform.position = new Vector3(
            playerBar.position.x,
            playerBar.position.y + playerBar.localScale.y / 2 + radius,//posição da barra+ metade da altura+ raio
            transform.position.z
        );
    }

    void LaunchBall()
    {
        //bola lançada
        isLaunched = true;
        //velocidade inicial, garantindo que seja sempre lançada para cima com uma leve variação aleatória para os lados
        velocity = new Vector2(Random.Range(-1f, 1f), 1f).normalized * initialSpeed;
    }

    void MoveBall()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }

    //definindo as paredes virtuais do jogo
    void HandleWallCollisions()
    {
        // Colisão com paredes laterais, verifica se a borda da bola ultrapassou o limite
        if (transform.position.x - radius < -8f)
        {
            transform.position = new Vector3(-8f + radius, transform.position.y, transform.position.z);
            velocity.x = -velocity.x;
        }
        else if (transform.position.x + radius > 8f)
        {
            transform.position = new Vector3(8f - radius, transform.position.y, transform.position.z);
            velocity.x = -velocity.x;
        }

        // Colisão com teto
        if (transform.position.y + radius > 5f)
        {
            transform.position = new Vector3(transform.position.x, 5f - radius, transform.position.z);
            velocity.y = -velocity.y;
        }
    }

    void HandlePaddleCollision()
    {
        Vector2 paddleSize = paddleCollisionSize;
        Vector2 paddleMin = new Vector2(
            playerBar.position.x - paddleSize.x / 2,
            playerBar.position.y - paddleSize.y / 2
        );
        Vector2 paddleMax = new Vector2(
            playerBar.position.x + paddleSize.x / 2,
            playerBar.position.y + paddleSize.y / 2
        );

        Vector2 ballMin = new Vector2(transform.position.x - radius, transform.position.y - radius);
        Vector2 ballMax = new Vector2(transform.position.x + radius, transform.position.y + radius);

        if (velocity.y < 0 &&
            ballMax.x > paddleMin.x &&
            ballMin.x < paddleMax.x &&
            ballMax.y > paddleMin.y &&
            ballMin.y < paddleMax.y)
        {
            cooldownTimer = collisionCooldown;

            // Calcula o ponto de impacto (-1 a 1)
            float hitPoint = (transform.position.x - playerBar.position.x) / (paddleSize.x / 2);
            hitPoint = Mathf.Clamp(hitPoint, -1f, 1f);

            // Calcula o ângulo de rebote
            float bounceAngle = hitPoint * (maxBounceAngle * Mathf.Deg2Rad);
            Vector2 newDirection = new Vector2(Mathf.Sin(bounceAngle), Mathf.Cos(bounceAngle));

            // Mantém a velocidade mas com direção nova
            velocity = newDirection.normalized * velocity.magnitude;

            // Garante que está indo para cima
            if (velocity.y < 0) velocity.y = -velocity.y;

            // Reposiciona a bola acima do paddle
            float newY = paddleMax.y + radius + 0.01f; // Pequeno offset extra
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    public void ResetBall()
    {
        isLaunched = false;
        velocity = Vector2.zero;
        cooldownTimer = 0f;
    }
    void OnDrawGizmos()
    {
        // Apenas desenha se o playerBar foi atribuído no Inspector
        if (playerBar != null)
        {
            // Pega o tamanho da barra exatamente como no seu cálculo de colisão
            Vector2 paddleSize = paddleCollisionSize;

            // Desenha um cubo (que vira um retângulo em 2D) com a mesma posição e tamanho da sua caixa de colisão
            Gizmos.color = Color.red; // Define a cor do desenho para vermelho
            Gizmos.DrawWireCube(playerBar.position, paddleSize);
        }
    }

    void HandleBrickCollisions()
    {
        for (int i = m_allBricks.Count - 1; i >= 0; i--)
        {
            GameObject brick = m_allBricks[i];

            if (brick == null) continue;

            Vector2 brickPos = brick.transform.position;
            Vector2 brickSize = brick.transform.localScale;

            float ballLeft = transform.position.x - radius;
            float ballRight = transform.position.x + radius;
            float ballTop = transform.position.y + radius;
            float ballBottom = transform.position.y - radius;

            float brickLeft = brickPos.x - brickSize.x / 2;
            float brickRight = brickPos.x + brickSize.x / 2;
            float brickTop = brickPos.y + brickSize.y / 2;
            float brickBottom = brickPos.y - brickSize.y / 2;

            if (ballRight > brickLeft && ballLeft < brickRight && ballTop > brickBottom && ballBottom < brickTop)
            {
                //direção do quique
                float overlapX = Mathf.Min(ballRight - brickLeft, brickRight - ballLeft);
                float overlapY = Mathf.Min(ballTop - brickBottom, brickBottom - ballBottom);

                if (overlapX < overlapY)
                {
                    velocity.x = -velocity.x;
                }
                else
                {
                    velocity.y = -velocity.y;
                }

                Destroy(brick.gameObject);
                m_allBricks.RemoveAt(i);

                break;
            }
        }
    }
}
