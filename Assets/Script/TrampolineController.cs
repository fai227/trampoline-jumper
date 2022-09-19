using UnityEngine;
using DG.Tweening;

public class TrampolineController : MonoBehaviour
{
    
    private static float bounceStrength = 0.3f;
    private static float fadeDuration = 0.5f;

    [Header("Clips")]
    [SerializeField] private AudioClip[] boundClips;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            //�v���C���[���΂�
            collision.rigidbody.AddForce(transform.up * (20f - transform.localScale.x) * bounceStrength, ForceMode2D.Impulse);

            //�����蔻�������
            GetComponent<BoxCollider2D>().isTrigger = true;

            //�g�����|�����̃t�F�[�h�A�E�g
            foreach(SpriteRenderer childRenderer in transform.parent.GetComponentsInChildren(typeof(SpriteRenderer)))
            {
                childRenderer.DOFade(0f, fadeDuration);
            }
            DOVirtual.DelayedCall(fadeDuration, () =>
            {
                Destroy(transform.parent.gameObject);
            });

            //�N���b�v�Đ�
            if(transform.localScale.x < 5f)
            {
                SoundManager.PlayOneShot(boundClips[0]);
            }
            else if(transform.localScale.x < 10f)
            {
                SoundManager.PlayOneShot(boundClips[1]);
            }
            else
            {
                SoundManager.PlayOneShot(boundClips[2]);
            }
        }
    }
}
