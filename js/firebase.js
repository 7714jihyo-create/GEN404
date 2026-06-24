/* ═══════════════════════════════════════════
   firebase.js – Firebase Realtime DB 연동
   공개 무료 프로젝트 (테스트용 공개 DB)
   ═══════════════════════════════════════════ */

/* ── Firebase 설정 (공개 테스트 프로젝트) ─── */
const firebaseConfig = {
  apiKey:            "AIzaSyDummyKeyForPublicTestProject404",
  authDomain:        "hospital404-game.firebaseapp.com",
  databaseURL:       "https://hospital404-game-default-rtdb.firebaseio.com",
  projectId:         "hospital404-game",
  storageBucket:     "hospital404-game.appspot.com",
  messagingSenderId: "000000000000",
  appId:             "1:000000000000:web:000000000000000000000000",
};

/* ──────────────────────────────────────────
   Firebase를 사용할 수 없을 때 로컬 폴백 모드
   (인터넷 없음 / DB 연결 실패 시 자동 전환)
────────────────────────────────────────── */
let DB = null;
let FB_AVAILABLE = false;

(function initFirebase() {
  try {
    if (typeof firebase !== 'undefined') {
      firebase.initializeApp(firebaseConfig);
      DB = firebase.database();
      /* 연결 테스트 */
      DB.ref('.info/connected').on('value', snap => {
        FB_AVAILABLE = snap.val() === true;
      });
    }
  } catch (e) {
    console.warn('Firebase 초기화 실패 – 로컬 모드로 실행합니다.', e);
    FB_AVAILABLE = false;
  }
})();

/* ══════════════════════════════════════════
   FBDB 래퍼 – 실패 시 로컬 시뮬레이션으로 폴백
   (주의: _LOCAL_ROOMS 참조 에러 수정됨)
══════════════════════════════════════════ */
const FBDB = {

  /* ── 방 생성 ── */
  async createRoom(roomId, hostName) {
    const room = {
      id:           roomId,
      host:         hostName,
      status:       'waiting',   // waiting | playing | round_start | diagnosing | simulating | results | final
      currentRound: 0,
      players:      {},
      patient:      null,
      diagnoses:    {},
      roundResults: [],
      createdAt:    Date.now(),
    };
    if (DB && FB_AVAILABLE) {
      await DB.ref(`rooms/${roomId}`).set(room);
    } else {
      LOCAL_ROOMS[roomId] = JSON.parse(JSON.stringify(room));
      _localNotify(roomId); // 로컬 생성 알림
    }
    return room;
  },

  /* ── 방 참가 ── */
  async joinRoom(roomId, playerId, playerName) {
    const playerData = {
      id:     playerId,
      name:   playerName,
      score:  0,
      scores: [],
      joinedAt: Date.now(),
    };
    if (DB && FB_AVAILABLE) {
      await DB.ref(`rooms/${roomId}/players/${playerId}`).set(playerData);
    } else {
      // _LOCAL_ROOMS -> LOCAL_ROOMS 수정
      if (!LOCAL_ROOMS[roomId]) throw new Error('방을 찾을 수 없습니다.');
      LOCAL_ROOMS[roomId].players[playerId] = playerData;
      _localNotify(roomId);
    }
  },

  /* ── 방 조회 ── */
  async getRoom(roomId) {
    if (DB && FB_AVAILABLE) {
      const snap = await DB.ref(`rooms/${roomId}`).once('value');
      return snap.val();
    } else {
      // _LOCAL_ROOMS -> LOCAL_ROOMS 수정
      return LOCAL_ROOMS[roomId] || null;
    }
  },

  /* ── 방 상태 업데이트 ── */
  async updateRoom(roomId, updates) {
    if (DB && FB_AVAILABLE) {
      await DB.ref(`rooms/${roomId}`).update(updates);
    } else {
      // _LOCAL_ROOMS -> LOCAL_ROOMS 수정
      if (!LOCAL_ROOMS[roomId]) return;
      Object.assign(LOCAL_ROOMS[roomId], updates);
      _localNotify(roomId);
    }
  },

  /* ── 진단서 제출 ── */
  async submitDiagnosis(roomId, playerId, diagData) {
    if (DB && FB_AVAILABLE) {
      await DB.ref(`rooms/${roomId}/diagnoses/${playerId}`).set(diagData);
    } else {
      if (!LOCAL_ROOMS[roomId]) return;
      if (!LOCAL_ROOMS[roomId].diagnoses) LOCAL_ROOMS[roomId].diagnoses = {};
      LOCAL_ROOMS[roomId].diagnoses[playerId] = diagData;
      _localNotify(roomId);
    }
  },

  /* ── 라운드 결과 저장 ── */
  async saveRoundResults(roomId, round, results) {
    if (DB && FB_AVAILABLE) {
      await DB.ref(`rooms/${roomId}/roundResults/${round - 1}`).set(results);
      // 점수 업데이트
      for (const res of results.playerResults) {
        await DB.ref(`rooms/${roomId}/players/${res.player.id}/score`).set(res.player.score);
        await DB.ref(`rooms/${roomId}/players/${res.player.id}/scores`).set(res.player.scores);
      }
    } else {
      if (!LOCAL_ROOMS[roomId]) return;
      if (!LOCAL_ROOMS[roomId].roundResults) LOCAL_ROOMS[roomId].roundResults = [];
      LOCAL_ROOMS[roomId].roundResults[round - 1] = results;
      for (const res of results.playerResults) {
        LOCAL_ROOMS[roomId].players[res.player.id].score  = res.player.score;
        LOCAL_ROOMS[roomId].players[res.player.id].scores = res.player.scores;
      }
      _localNotify(roomId);
    }
  },

  /* ── 방 실시간 리스너 ── */
  onRoomChange(roomId, callback) {
    if (DB && FB_AVAILABLE) {
      const ref = DB.ref(`rooms/${roomId}`);
      ref.on('value', snap => callback(snap.val()));
      return () => ref.off('value');
    } else {
      // 로컬 폴백: 이벤트 에뮬레이터
      const listenerId = `${roomId}_${Date.now()}`;
      LOCAL_LISTENERS[listenerId] = { roomId, callback };
      // 즉시 현재 상태 전달
      if (LOCAL_ROOMS[roomId]) callback(JSON.parse(JSON.stringify(LOCAL_ROOMS[roomId])));
      return () => { delete LOCAL_LISTENERS[listenerId]; };
    }
  },

  /* ── 방 삭제 (게임 종료 후) ── */
  async removeRoom(roomId) {
    if (DB && FB_AVAILABLE) {
      await DB.ref(`rooms/${roomId}`).remove();
    } else {
      delete LOCAL_ROOMS[roomId];
      _localNotify(roomId);
    }
  },
};

/* ══════════════════════════════════════════
   로컬 폴백 시뮬레이터
   (같은 기기 내 멀티 탭 시뮬레이션)
══════════════════════════════════════════ */
const LOCAL_ROOMS     = {};
const LOCAL_LISTENERS = {};

function _localNotify(roomId) {
  const room = LOCAL_ROOMS[roomId];
  // room이 없어도(삭제된 경우) 리스너에게 알려야 함
  Object.values(LOCAL_LISTENERS).forEach(({ rid, callback, roomId: lRoomId }) => {
    if ((rid || lRoomId) === roomId) {
      callback(room ? JSON.parse(JSON.stringify(room)) : null);
    }
  });
  // BroadcastChannel으로 다른 탭에도 전파
  try {
    const bc = new BroadcastChannel('hospital404');
    bc.postMessage({ type: 'roomUpdate', roomId, room: room ? JSON.parse(JSON.stringify(room)) : null });
    bc.close();
  } catch (e) {}
}

/* BroadcastChannel 수신 (다른 탭에서 온 업데이트) */
try {
  const bc = new BroadcastChannel('hospital404');
  bc.onmessage = (e) => {
    if (e.data?.type === 'roomUpdate') {
      const { roomId, room } = e.data;
      if (room === null) {
        delete LOCAL_ROOMS[roomId];
      } else {
        LOCAL_ROOMS[roomId] = room;
      }
      Object.values(LOCAL_LISTENERS).forEach(({ lRoomId, roomId: lid, callback }) => {
        if ((lRoomId || lid) === roomId) {
          callback(room ? JSON.parse(JSON.stringify(room)) : null);
        }
      });
    }
  };
} catch (e) {}

/* ── 방 코드 생성 ── */
function generateRoomCode() {
  const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
  let code = '';
  for (let i = 0; i < 4; i++) code += chars[Math.floor(Math.random() * chars.length)];
  return code;
}

/* ── 플레이어 ID 생성 (로컬 스토리지 기반) ── */
function getOrCreatePlayerId() {
  let id = localStorage.getItem('hospital404_pid');
  if (!id) {
    id = 'p_' + Date.now() + '_' + Math.random().toString(36).substr(2, 6);
    localStorage.setItem('hospital404_pid', id);
  }
  return id;
}
