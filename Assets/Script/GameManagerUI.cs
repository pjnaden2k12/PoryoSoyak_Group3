using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerUI : MonoBehaviour
{
    public Button playButton;
    public Button pauseButton;
    private bool isGamePlaying = false;

    public void OnPlayPauseButtonPressed()
    {
        // Thêm hiệu ứng scale cho nút Play và Pause khi nhấn
        playButton.transform.DOScale(1.1f, 0.1f).OnComplete(() => {
            playButton.transform.DOScale(1f, 0.1f);
        });

        pauseButton.transform.DOScale(1.1f, 0.1f).OnComplete(() => {
            pauseButton.transform.DOScale(1f, 0.1f);
        });

        isGamePlaying = !isGamePlaying;

        if (isGamePlaying)
        {
            // Khi nhấn Play, sẽ bắt đầu trò chơi
            playButton.gameObject.SetActive(false);  // Ẩn Play
            pauseButton.gameObject.SetActive(true);  // Hiện Pause
            MedicineAutoMove.isPlayPressed = true;  // Kích hoạt việc di chuyển của medicine

            GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
            foreach (GameObject item in items)
            {
                DragMove dragMove = item.GetComponent<DragMove>();
                DragItem dragItem = item.GetComponent<DragItem>();

                bool isSnapped = false;

                // Kiểm tra xem item có snap hay không
                if (dragMove != null) isSnapped = dragMove.isSnapped;
                if (dragItem != null) isSnapped = dragItem.isSnapped;

                // Chỉ làm mờ item chưa snap
                if (!isSnapped)
                {
                    if (dragMove != null) dragMove.enabled = false;  // Tắt khả năng di chuyển
                    if (dragItem != null) dragItem.enabled = false;  // Tắt khả năng di chuyển

                    Collider2D col = item.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;  // Tắt collider

                    SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = 0.4f;  // Đặt alpha = 0.4 (làm mờ item)
                        sr.color = c;
                    }
                }
                else
                {
                    // Nếu item đã snap, vẫn bật các tính năng di chuyển
                    if (dragMove != null) dragMove.enabled = true;  // Bật khả năng di chuyển
                    if (dragItem != null) dragItem.enabled = true;  // Bật khả năng di chuyển

                    Collider2D col = item.GetComponent<Collider2D>();
                    if (col != null) col.enabled = true;  // Bật collider

                    SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = 1f;  // Đặt alpha = 1 (hiển thị rõ ràng)
                        sr.color = c;
                    }
                }
            }
        }
        else
        {
            // Khi nhấn Pause, sẽ tạm dừng trò chơi
            playButton.gameObject.SetActive(true);  // Hiện Play
            pauseButton.gameObject.SetActive(false);  // Ẩn Pause
            MedicineAutoMove.isPlayPressed = false;  // Dừng trò chơi

            GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
            foreach (GameObject item in items)
            {
                DragMove dragMove = item.GetComponent<DragMove>();
                DragItem dragItem = item.GetComponent<DragItem>();

                bool isSnapped = false;

                if (dragMove != null) isSnapped = dragMove.isSnapped;
                if (dragItem != null) isSnapped = dragItem.isSnapped;

                // Khi Pause, tất cả các item sẽ được bật lại và có thể di chuyển
                if (dragMove != null) dragMove.enabled = true;  // Bật khả năng di chuyển
                if (dragItem != null) dragItem.enabled = true;  // Bật khả năng di chuyển

                Collider2D col = item.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;  // Bật collider

                SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = 1f;  // Đặt alpha = 1 (hiển thị rõ ràng)
                    sr.color = c;
                }
            }
        }
    }

    public void OnReplayButtonPressed()
    {
        MedicineAutoMove.isPlayPressed = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
