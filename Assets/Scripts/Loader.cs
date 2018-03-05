using UnityEngine;

public class Loader : MonoBehaviour {

    public GameObject gameManager;

    private void Awake()
    {
        if (GameManager.Instance == null)
        {
            Instantiate(gameManager);
        }
    }

    public class KeyValuePair<TKey, TValue>
    {
        public TKey key;
        public TValue value;

        public KeyValuePair(TKey _key, TValue _value)
        {
            key = _key;
            value = _value;
        }

        public void Print()
        {
            Debug.Log("Key: " + key.ToString());
            Debug.Log("Value: " + value.ToString());
        }
    }

}
